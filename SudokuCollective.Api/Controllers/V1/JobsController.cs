﻿using System;
using System.Net;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Hangfire.Storage.Monitoring;
using Hangfire;
using Hangfire.Storage;
using Newtonsoft.Json;
using SudokuCollective.Api.Utilities;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models.Params;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace SudokuCollective.Api.Controllers.V1
{
    /// <summary>
    /// Jobs Controller Class
    /// </summary>
    /// <remarks>
    /// Jobs Controller Class
    /// </remarks>
    [AllowAnonymous]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class JobsController(
        IRequestService requestService,
        ILogger<JobsController> logger,
        IMonitoringApi monitoringApi = null,
        IStorageConnection storageConnection = null) : ControllerBase
    {
        private readonly IRequestService _requestService = requestService;
        private readonly ILogger<JobsController> _logger = logger;
        private IMonitoringApi _monitoringApi = monitoringApi;
        private IStorageConnection _connection = storageConnection;

        /// <summary>
        /// An endpoint which gets the job results by the job id.
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns>The results of the indicated job.</returns>
        /// <response code="200">Returns a result object for success if isSuccess is true.</response>
        /// <response code="400">Returns a result object for failure if isSuccess is false.</response>
        /// <remarks>
        /// This endpoint is used to obtain the result for the completed job.  Please provide the jobId as a query parameter.
        /// </remarks>
        [AllowAnonymous]
        [HttpGet, Route("Get")]
        public ActionResult<Result> Get([FromQuery] string jobId)
        {
            try
            {
                ArgumentException.ThrowIfNullOrEmpty(jobId);

                Result result;

                _monitoringApi ??= JobStorage.Current.GetMonitoringApi();

                JobDetailsDto job = 
                    _monitoringApi.JobDetails(jobId) ?? 
                    throw new ArgumentException(string.Format(JobsMessages.JobIdIsInvalid, jobId));

                if (job.History[0].StateName.Equals("Succeeded"))
                {
                    string jobResult = job.History[0].Data["Result"];

                    result = JsonConvert.DeserializeObject<Result>(jobResult, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });

                    if (result.IsSuccess)
                    {
                        if (result.Message.Contains("was created") && result.Payload[0] is IDomainEntity)
                        {
                            result.Message = ControllerMessages.StatusCode201(result.Message);

                            return StatusCode((int)HttpStatusCode.Created, result);
                        }
                        else
                        {
                            result.Message = ControllerMessages.StatusCode200(result.Message);

                            return Ok(result);
                        }
                    }
                    else
                    {
                        result.Message = ControllerMessages.StatusCode400(result.Message);

                        return NotFound(result);
                    }
                }
                else
                {
                    result = new Result
                    {
                        IsSuccess = false,
                        Message = ControllerMessages.StatusCode400(
                            string.Format(
                                JobsMessages.JobRetrievalFailedDueToJobState,
                                jobId,
                                job.History[0].StateName.ToLower()))
                    };

                    return NotFound(result);
                }
            }
            catch (Exception e)
            {
                return ControllerUtilities.ProcessException(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }

        /// <summary>
        /// An endpoint which gets the job status identified by the job id.
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns>The status of the indicated job.</returns>
        /// <response code="200">Returns a result object indicating a job status of 'scheduled', 'processing' or 'succeeded'.</response>
        /// <remarks>
        /// This endpoint is used to poll the job client for the status of the indicated job.  Please provide the jobId as a query parameter.
        /// The result will provide an updated status along with the message describing the current job status.
        /// </remarks>
        [AllowAnonymous]
        [HttpGet, Route("Poll")]
        public ActionResult<Result> Poll([FromQuery] string jobId)
        {
            try
            {
                ArgumentException.ThrowIfNullOrEmpty(jobId);

                var result = new Result();

                _connection ??= JobStorage.Current.GetConnection();

                var jobData = _connection.GetJobData(jobId) ?? throw new ArgumentException(string.Format(JobsMessages.JobIdIsInvalid, jobId));

                if (jobData.State.Equals("Succeeded"))
                {
                    result.IsSuccess = true;
                    result.Message = ControllerMessages.StatusCode200(
                        string.Format(JobsMessages.JobIsCompletedWithStatus,
                        jobId,
                        jobData.State.ToLower()));

                    return Ok(result);
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = ControllerMessages.StatusCode200(string.Format(
                        JobsMessages.JobIsNotCompletedWithStatus,
                        jobId,
                        jobData.State.ToLower()));

                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return ControllerUtilities.ProcessException(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }
    }
}

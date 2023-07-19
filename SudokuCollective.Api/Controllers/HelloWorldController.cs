using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models.Params;

namespace SudokuCollective.Api.Controllers
{
    /// <summary>
    /// Hello World Controller Class
    /// </summary>
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class HelloWorldController : ControllerBase
    {
        /// <summary>
        /// An example endpoint of how to connect to an API for new developers, does not require a login.
        /// </summary>
        /// <returns>A simple message outlining the response model, if a param was included it is echoed in the response.</returns>
        /// <response code="200">Returns a result object with a message summarizing the response, if a param was included it is echoed in the message.</response>
        /// <response code="400">Returns a result object with the message summarizing why the request could not be completed.</response>
        /// <remarks>
        /// The Get endpoint does not require an authorization token.  This endpoint serves as a simple example of how to connect to an 
        /// api for new developers.  The endpoint accepts a query parameter of 'param', if param is empty it issues a canned response.
        /// 
        /// An example request is as follows: 
        /// ```
        /// https://sudokucollective.com/api/helloWorld?param=hello_world
        /// ```
        /// The standard API response is as follows:
        /// ```
        /// {
        ///   "isSuccess": true,                                        // An indicator if the request was successful
        ///   "isFromCache": false,                                     // An indicator if the payload was obtained from the cache
        ///   "message": "Status Code 200: You Submitted: hello_world", // A brief description of the result
        ///   "payload": []                                             // The payload response you requested, returned as an array of objects
        /// }
        /// ```
        /// </remarks>
        [AllowAnonymous]
        [HttpGet]
        public ActionResult<Result> Get(string param = null)
        {
            var result = new Result();

            try
            {
                result.IsSuccess = true;

                if (string.IsNullOrEmpty(param))
                {
                    result.Message = ControllerMessages.StatusCode200(ControllerMessages.HelloWorld);
                }
                else
                {
                    result.Message = ControllerMessages.StatusCode200(ControllerMessages.Echo(param));
                }

                return Ok(result);
            }
            catch(Exception e)
            {
                result.IsSuccess = false;

                result.Message = ControllerMessages.StatusCode400(e.Message);

                return BadRequest(result);
            }
        }
    }
}

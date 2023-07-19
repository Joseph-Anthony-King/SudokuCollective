using System;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models.Params;

namespace SudokuCollective.Api.Controllers
{
	/// <summary>
	/// Error Controller Class
	/// </summary>
	[AllowAnonymous]
	[Route("[controller]")]
	[ApiController]
	public class ErrorController : ControllerBase
	{
		/// <summary>
		/// An endpoint to output get errors and ensure they conform to this api structure
		/// </summary>
		/// <returns>A result object with a message describing the error.</returns>
		/// <response code="400">Returns a result object with a message describing the error.</response>
		[AllowAnonymous]
		[Route("/error")]
		[HttpGet]
		public IActionResult HandleGetError()
		{
			return HandleError();
		}

		/// <summary>
		/// An endpoint to output post errors and ensure they conform to this api structure
		/// </summary>
		/// <returns>A result object with a message describing the error.</returns>
		/// <response code="400">Returns a result object with a message describing the error.</response>
		[AllowAnonymous]
		[Route("/error")]
		[HttpPost]
		public IActionResult HandlePostError()
		{
			return HandleError();
		}

		/// <summary>
		/// An endpoint to output delete errors and ensure they conform to this api structure
		/// </summary>
		/// <returns>A result object with a message describing the error.</returns>
		/// <response code="400">Returns a result object with a message describing the error.</response>
		[AllowAnonymous]
		[Route("/error")]
		[HttpDelete]
		public IActionResult HandleDeleteError()
		{
			return HandleError();
		}

		/// <summary>
		/// An endpoint to output put errors and ensure they conform to this api structure
		/// </summary>
		/// <returns>A result object with a message describing the error.</returns>
		/// <response code="400">Returns a result object with a message describing the error.</response>
		[AllowAnonymous]
		[Route("/error")]
		[HttpPut]
		public IActionResult HandlePutError()
		{
			return HandleError();
		}

		/// <summary>
		/// An endpoint to output patch errors and ensure they conform to this api structure
		/// </summary>
		/// <returns>A result object with a message describing the error.</returns>
		/// <response code="400">Returns a result object with a message describing the error.</response>
		[AllowAnonymous]
		[Route("/error")]
		[HttpPatch]
		public IActionResult HandlePatchError()
		{
			return HandleError();
		}

		private IActionResult HandleError()
		{
			var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;

			var isArgumentException  = exceptionHandlerFeature.Error.GetType() == typeof(ArgumentException) || 
				exceptionHandlerFeature.Error.GetType() == typeof(ArgumentNullException);

			var message = isArgumentException ? 
				ControllerMessages.StatusCode400(exceptionHandlerFeature.Error.Message) : 
				ControllerMessages.StatusCode500(exceptionHandlerFeature.Error.Message);

			var result = new Result
			{
					IsSuccess = false,
					Message = message
			};

			return isArgumentException ?  
				BadRequest(result) : 
				this.StatusCode((int)HttpStatusCode.InternalServerError, result);
		}
	}
}


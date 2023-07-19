using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SudokuCollective.Api.Controllers;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Test.HtttpContext;

namespace SudokuCollective.Test.TestCases.Controllers
{
    public class ErrorControllerShould
    {
        private ErrorController sut;

        [Test, Category("Controllers")]
        public void ProcessesGetRequestArgumentExceptions()
        {
            // Arrange
            sut = new ErrorController { ControllerContext = MockWebContext.BasicContext("Parameter Invalid") };

            // Act
            var actionResult = sut.HandleGetError();
            var result = (Result)((BadRequestObjectResult)actionResult).Value;
            var message = result.Message;
            var statusCode = ((BadRequestObjectResult)actionResult).StatusCode;

            // Assert
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 400: Parameter Invalid"));
            Assert.That(statusCode, Is.EqualTo(400));
        }

        [Test, Category("Controllers")]
        public void ProcessesGetRequestSystemExceptions()
        {
            // Arrange
            sut = new ErrorController { ControllerContext = MockWebContext.BasicContext() };

            // Act
            var actionResult = sut.HandleGetError();
            var result = (Result)((ObjectResult)actionResult).Value;
            var message = result.Message;
            var statusCode = ((ObjectResult)actionResult).StatusCode;

            // Assert
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 500: Null Reference Exception"));
            Assert.That(statusCode, Is.EqualTo(500));
        }

        [Test, Category("Controllers")]
        public void ProcessesPostRequestArgumentExceptions()
        {
            // Arrange
            sut = new ErrorController { ControllerContext = MockWebContext.BasicContext("Parameter Invalid") };

            // Act
            var actionResult = sut.HandlePostError();
            var result = (Result)((BadRequestObjectResult)actionResult).Value;
            var message = result.Message;
            var statusCode = ((BadRequestObjectResult)actionResult).StatusCode;

            // Assert
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 400: Parameter Invalid"));
            Assert.That(statusCode, Is.EqualTo(400));
        }

        [Test, Category("Controllers")]
        public void ProcessesPostRequestSystemExceptions()
        {
            // Arrange
            sut = new ErrorController { ControllerContext = MockWebContext.BasicContext() };

            // Act
            var actionResult = sut.HandlePostError();
            var result = (Result)((ObjectResult)actionResult).Value;
            var message = result.Message;
            var statusCode = ((ObjectResult)actionResult).StatusCode;

            // Assert
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 500: Null Reference Exception"));
            Assert.That(statusCode, Is.EqualTo(500));
        }

        [Test, Category("Controllers")]
        public void ProcessesDeleteRequestArgumentExceptions()
        {
            // Arrange
            sut = new ErrorController { ControllerContext = MockWebContext.BasicContext("Parameter Invalid") };

            // Act
            var actionResult = sut.HandleDeleteError();
            var result = (Result)((BadRequestObjectResult)actionResult).Value;
            var message = result.Message;
            var statusCode = ((BadRequestObjectResult)actionResult).StatusCode;

            // Assert
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 400: Parameter Invalid"));
            Assert.That(statusCode, Is.EqualTo(400));
        }

        [Test, Category("Controllers")]
        public void ProcessesDeleteRequestSystemExceptions()
        {
            // Arrange
            sut = new ErrorController { ControllerContext = MockWebContext.BasicContext() };

            // Act
            var actionResult = sut.HandleDeleteError();
            var result = (Result)((ObjectResult)actionResult).Value;
            var message = result.Message;
            var statusCode = ((ObjectResult)actionResult).StatusCode;

            // Assert
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 500: Null Reference Exception"));
            Assert.That(statusCode, Is.EqualTo(500));
        }

        [Test, Category("Controllers")]
        public void ProcessesPutRequestArgumentExceptions()
        {
            // Arrange
            sut = new ErrorController { ControllerContext = MockWebContext.BasicContext("Parameter Invalid") };

            // Act
            var actionResult = sut.HandlePutError();
            var result = (Result)((BadRequestObjectResult)actionResult).Value;
            var message = result.Message;
            var statusCode = ((BadRequestObjectResult)actionResult).StatusCode;

            // Assert
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 400: Parameter Invalid"));
            Assert.That(statusCode, Is.EqualTo(400));
        }

        [Test, Category("Controllers")]
        public void ProcessesPutRequestSystemExceptions()
        {
            // Arrange
            sut = new ErrorController { ControllerContext = MockWebContext.BasicContext() };

            // Act
            var actionResult = sut.HandlePutError();
            var result = (Result)((ObjectResult)actionResult).Value;
            var message = result.Message;
            var statusCode = ((ObjectResult)actionResult).StatusCode;

            // Assert
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 500: Null Reference Exception"));
            Assert.That(statusCode, Is.EqualTo(500));
        }

        [Test, Category("Controllers")]
        public void ProcessesPatchRequestArgumentExceptions()
        {
            // Arrange
            sut = new ErrorController { ControllerContext = MockWebContext.BasicContext("Parameter Invalid") };

            // Act
            var actionResult = sut.HandlePatchError();
            var result = (Result)((BadRequestObjectResult)actionResult).Value;
            var message = result.Message;
            var statusCode = ((BadRequestObjectResult)actionResult).StatusCode;

            // Assert
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 400: Parameter Invalid"));
            Assert.That(statusCode, Is.EqualTo(400));
        }

        [Test, Category("Controllers")]
        public void ProcessesPatchRequestSystemExceptions()
        {
            // Arrange
            sut = new ErrorController { ControllerContext = MockWebContext.BasicContext() };

            // Act
            var actionResult = sut.HandlePatchError();
            var result = (Result)((ObjectResult)actionResult).Value;
            var message = result.Message;
            var statusCode = ((ObjectResult)actionResult).StatusCode;

            // Assert
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 500: Null Reference Exception"));
            Assert.That(statusCode, Is.EqualTo(500));
        }
    }
}

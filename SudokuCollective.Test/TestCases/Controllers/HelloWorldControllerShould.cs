using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SudokuCollective.Api.Controllers;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Data.Models.Params;

namespace SudokuCollective.Test.TestCases.Controllers
{
    public class HelloWorldControllerShould
    {
        private HelloWorldController sut;

        [SetUp]
        public void SetUp()
        {
            sut = new HelloWorldController();
        }

        [Test, Category("Controllers")]
        public void ReturnAMessage()
        {
            // Arrange

            // Act
            var actionResult = sut.Get();
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var success = result.IsSuccess;
            var message = result.Message;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.TypeOf<ActionResult<Result>>());
            Assert.That(result, Is.TypeOf<Result>());
            Assert.That(success, Is.True);
            Assert.That(message, Is.EqualTo("Status Code 200: Hello World from Sudoku Collective!"));
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test, Category("Controllers")]
        public void EchoParams()
        {
            // Arrange

            // Act
            var actionResult = sut.Get("hello_world");
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var success = result.IsSuccess;
            var message = result.Message;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.TypeOf<ActionResult<Result>>());
            Assert.That(result, Is.TypeOf<Result>());
            Assert.That(success, Is.True);
            Assert.That(message, Is.EqualTo("Status Code 200: You Submitted: hello_world"));
            Assert.That(statusCode, Is.EqualTo(200));
        }
    }
}

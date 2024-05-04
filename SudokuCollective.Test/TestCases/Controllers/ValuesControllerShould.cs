using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Api.Controllers.V1;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Test.Services;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Controllers
{
    public class ValuesControllerShould
    {
        private DatabaseContext context;
        private ValuesController sut;
        private MockedValuesService mockedValuesService;
        private Mock<ILogger<ValuesController>> mockedLogger;
        private Mock<IWebHostEnvironment> mockWebHostEnvironment;

        [SetUp]
        public async Task SetUp()
        {
            context = await TestDatabase.GetDatabaseContext();
            mockedValuesService = new MockedValuesService(context);
            mockedLogger = new Mock<ILogger<ValuesController>>();
            mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
            sut = new ValuesController(
                mockedValuesService.Request.Object,
                mockedLogger.Object,
                mockWebHostEnvironment.Object);
        }

        [Test, Category("Controller")]
        public async Task GetValues()
        {
            // Arrange and Act
            var actionResult = await sut.GetAsync();
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.TypeOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test, Category("Controller")]
        public void GetAListOfReleaseEnvironments()
        {
            // Arrange and Act
            var actionResult = sut.GetReleaseEnvironments();
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.TypeOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test, Category("Controller")]
        public void GetAListOfTimeFrames()
        {
            // Arrange and Act
            var actionResult = sut.GetTimeFrames();
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.TypeOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test, Category("Controller")]
        public void GetAListOfSortValues()
        {
            // Arrange and Act
            var actionResult = sut.GetSortValues();
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.TypeOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(statusCode, Is.EqualTo(200));
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SudokuCollective.Api.Controllers;
using SudokuCollective.Data.Models;
using SudokuCollective.Test.Services;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Controllers
{
    public class PasswordResetControllerShould
    {
        private DatabaseContext context;
        private PasswordResetController sut;
        private MockedUsersService mockedUsersService;
        private MockedAppsService mockedAppsService;
        private string passwordResetToken;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();

            mockedUsersService = new MockedUsersService(context);
            mockedAppsService = new MockedAppsService(context);

            sut = new PasswordResetController(
                mockedUsersService.SuccessfulRequest.Object,
                mockedAppsService.SuccessfulRequest.Object);

            passwordResetToken = Guid.NewGuid().ToString();
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyInitiateResetPasswordRequests()
        {
            // Arrange

            // Act
            var result = await sut.Index(passwordResetToken);

            // Assert
            Assert.That(result, Is.InstanceOf<ActionResult>());
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyProcessResetPasswordRequests()
        {
            // Arrange
            var appId = context.Apps.Where(a => a.Id == 1).Select(a => a.Id);

            // Act
            var result = await sut.Result(
                TestObjects.GetPasswordReset());

            // Assert
            Assert.That(result, Is.InstanceOf<ActionResult>());
        }
    }
}

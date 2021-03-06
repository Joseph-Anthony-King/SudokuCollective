﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SudokuCollective.Api.Controllers;
using SudokuCollective.Data.Models;
using SudokuCollective.Test.MockServices;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Controllers
{
    public class PasswordResetControllerShould
    {
        private DatabaseContext context;
        private PasswordResetController sut;
        private MockUsersService mockUsersService;
        private MockAppsService mockAppsService;
        private string passwordResetToken;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();

            mockUsersService = new MockUsersService(context);
            mockAppsService = new MockAppsService(context);

            sut = new PasswordResetController(
                mockUsersService.UsersServiceSuccessfulRequest.Object,
                mockAppsService.AppsServiceSuccessfulRequest.Object);

            passwordResetToken = Guid.NewGuid().ToString();
        }

        [Test]
        [Category("Controllers")]
        public void SuccessfullyInitiateResetPasswordRequests()
        {
            // Arrange

            // Act
            var result = sut.Index(passwordResetToken);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<ActionResult>());
        }

        [Test]
        [Category("Controllers")]
        public void SuccessfullyProcessResetPasswordRequests()
        {
            // Arrange
            var appId = context.Apps.Where(a => a.Id == 1).Select(a => a.Id);

            // Act
            var result = sut.Result(
                TestObjects.GetPasswordReset());

            // Assert
            Assert.That(result.Result, Is.InstanceOf<ActionResult>());
        }
    }
}

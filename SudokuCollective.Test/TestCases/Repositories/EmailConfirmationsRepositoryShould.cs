using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Repos;
using SudokuCollective.Test.Services;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Repositories
{
    public class EmailConfirmationsRepositoryShould
    {
        private DatabaseContext context;
        private MockedRequestService mockedRequestService;
        private Mock<ILogger<EmailConfirmationsRepository<EmailConfirmation>>> mockedLogger;
        private IEmailConfirmationsRepository<EmailConfirmation> sut;
        private EmailConfirmation newEmailConfirmation;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();
            mockedRequestService = new MockedRequestService();
            mockedLogger = new Mock<ILogger<EmailConfirmationsRepository<EmailConfirmation>>>();

            sut = new EmailConfirmationsRepository<EmailConfirmation>(
                context,
                mockedRequestService.SuccessfulRequest.Object,
                mockedLogger.Object);

            newEmailConfirmation = new EmailConfirmation(2, 1);
        }

        [Test, Category("Repository")]
        public async Task CreateEmailConfirmations()
        {
            // Arrange and Act
            var result = await sut.CreateAsync(newEmailConfirmation);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((EmailConfirmation)result.Object, Is.InstanceOf<EmailConfirmation>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfCreateEmailConfirmationsFails()
        {
            // Arrange
            newEmailConfirmation.Id = 2;

            // Act
            var result = await sut.CreateAsync(newEmailConfirmation);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task GetEmailConfirmationsByToken()
        {
            // Arrange
            var token = context.EmailConfirmations.Select(ec => ec.Token).FirstOrDefault();

            // Act
            var result = await sut.GetAsync(token);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((EmailConfirmation)result.Object, Is.InstanceOf<EmailConfirmation>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetByTokenFails()
        {
            // Arrange and Act
            var result = await sut.GetAsync(Guid.NewGuid().ToString());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Object, Is.Null);
        }

        [Test, Category("Repository")]
        public async Task GetAllEmailConfirmations()
        {
            // Arrange and Act
            var result = await sut.GetAllAsync();

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Objects.ConvertAll(ec => (EmailConfirmation)ec), Is.InstanceOf<List<EmailConfirmation>>());
        }

        [Test, Category("Repository")]
        public async Task UpdateEmailConfirmations()
        {
            // Arrange
            var emailConfirmation = context.EmailConfirmations.FirstOrDefault(ec => ec.Id == 1);
            emailConfirmation.OldEmailAddress = string.Format("UPDATED{0}", emailConfirmation.OldEmailAddress);

            // Act
            var result = await sut.UpdateAsync(emailConfirmation);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Object, Is.InstanceOf<EmailConfirmation>());
            Assert.That(((EmailConfirmation)result.Object).OldEmailAddress, Is.EqualTo(emailConfirmation.OldEmailAddress));
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfUpdateEmailConfirmationsFails()
        {
            // Arrange and Act
            var result = await sut.UpdateAsync(newEmailConfirmation);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Object, Is.Null);
        }

        [Test, Category("Repository")]
        public async Task DeleteEmailConfirmations()
        {
            // Arrange
            var emailConfirmation = context.EmailConfirmations.FirstOrDefault(ec => ec.Id == 1);

            // Act
            var result = await sut.DeleteAsync(emailConfirmation);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfDeleteEmailConfirmationsFails()
        {
            // Arrange and Act
            var result = await sut.DeleteAsync(newEmailConfirmation);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task ConfirmItHasAnEmailConfirmation()
        {
            // Arrange and Act
            var result = await sut.HasEntityAsync(1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ConfirmEmailConfirmationsByUserAndAppId()
        {
            // Arrange
            var emailConfirmation = context.EmailConfirmations.FirstOrDefault();

            // Act
            var result = await sut.HasOutstandingEmailConfirmationAsync(emailConfirmation.UserId, emailConfirmation.AppId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Repository")]
        public async Task GetEmailConfirmationsByUserAndAppId()
        {
            // Arrange
            var emailConfirmation = context.EmailConfirmations.FirstOrDefault();

            // Act
            var result = await sut.RetrieveEmailConfirmationAsync(emailConfirmation.UserId, emailConfirmation.AppId);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((EmailConfirmation)result.Object, Is.InstanceOf<EmailConfirmation>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetEmailConfirmationsByUserAndAppIdFails()
        {
            // Arrange
            var emailConfirmation = context.EmailConfirmations.FirstOrDefault();

            // Act
            var result = await sut.RetrieveEmailConfirmationAsync(emailConfirmation.UserId + 3, emailConfirmation.AppId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }
    }
}

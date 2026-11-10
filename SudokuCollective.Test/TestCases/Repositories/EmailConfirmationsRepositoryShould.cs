using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Core.Enums;
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

            newEmailConfirmation = new EmailConfirmation(EmailConfirmationType.OLDEMAILCONFIRMED, 2, 1);
        }

        [Test, Category("Repository")]
        public async Task CreateEmailConfirmations()
        {
            // Arrange and Act
            var result = await sut.CreateAsync(newEmailConfirmation);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((EmailConfirmation)result.DomainObject, Is.InstanceOf<EmailConfirmation>());
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
        public async Task AddEmailConfirmation()
        {
            // Arrange

            // Act
            var result = await sut.AddAsync(newEmailConfirmation);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.DomainObject, Is.InstanceOf<EmailConfirmation>());
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
            Assert.That((EmailConfirmation)result.DomainObject, Is.InstanceOf<EmailConfirmation>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetByTokenFails()
        {
            // Arrange and Act
            var result = await sut.GetAsync(Guid.NewGuid().ToString());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DomainObject, Is.Null);
        }

        [Test, Category("Repository")]
        public async Task GetEmailConfirmationById()
        {
            // Arrange

            // Act
            var result = await sut.GetAsync(1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.DomainObject, Is.InstanceOf<EmailConfirmation>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetEmailConfirmationByIdFails()
        {
            // Arrange

            // Act
            var result = await sut.GetAsync(999);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DomainObject, Is.Null);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetEmailConfirmationByIdReceivesInvalidId()
        {
            // Arrange

            // Act
            var result = await sut.GetAsync(0);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Exception, Is.Not.Null);
        }

        [Test, Category("Repository")]
        public async Task GetAllEmailConfirmations()
        {
            // Arrange and Act
            var result = await sut.GetAllAsync();

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.DomainObjects.ConvertAll(ec => (EmailConfirmation)ec), Is.InstanceOf<List<EmailConfirmation>>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetAllEmailConfirmationsFails()
        {
            // Arrange
            context.EmailConfirmations.RemoveRange(context.EmailConfirmations);
            await context.SaveChangesAsync();

            // Act
            var result = await sut.GetAllAsync();

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DomainObjects, Is.Empty);
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
            Assert.That(result.DomainObject, Is.InstanceOf<EmailConfirmation>());
            Assert.That(((EmailConfirmation)result.DomainObject).OldEmailAddress, Is.EqualTo(emailConfirmation.OldEmailAddress));
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfUpdateEmailConfirmationsFails()
        {
            // Arrange and Act
            var result = await sut.UpdateAsync(newEmailConfirmation);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DomainObject, Is.Null);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfUpdateEmailConfirmationHasDuplicateToken()
        {
            // Arrange
            var emailConfirmation = context.EmailConfirmations.First(ec => ec.Id == 1);
            emailConfirmation.Token = context.EmailConfirmations
                .First(ec => ec.Id == 2)
                .Token;

            // Act
            var result = await sut.UpdateAsync(emailConfirmation);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DomainObject, Is.Null);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfUpdateEmailConfirmationReceivesInvalidId()
        {
            // Arrange
            newEmailConfirmation.Id = 0;

            // Act
            var result = await sut.UpdateAsync(newEmailConfirmation);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Exception, Is.Not.Null);
        }

        [Test, Category("Repository")]
        public async Task UpdateRangeOfEmailConfirmations()
        {
            // Arrange
            var emailConfirmations = context.EmailConfirmations
                .Take(2)
                .ToList();

            // Act
            var result = await sut.UpdateRangeAsync(emailConfirmations);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfUpdateRangeContainsUnknownEmailConfirmation()
        {
            // Arrange
            newEmailConfirmation.Id = 999;

            // Act
            var result = await sut.UpdateRangeAsync([newEmailConfirmation]);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfUpdateRangeReceivesInvalidEmailConfirmation()
        {
            // Arrange

            // Act
            var result = await sut.UpdateRangeAsync([newEmailConfirmation]);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Exception, Is.Not.Null);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfUpdateRangeReceivesNull()
        {
            // Arrange

            // Act
            var result = await sut.UpdateRangeAsync(null);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Exception, Is.Not.Null);
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
        public async Task ReturnFalseIfDeleteEmailConfirmationReceivesInvalidId()
        {
            // Arrange

            // Act
            var result = await sut.DeleteAsync(newEmailConfirmation);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Exception, Is.Not.Null);
        }

        [Test, Category("Repository")]
        public async Task DeleteRangeOfEmailConfirmations()
        {
            // Arrange
            var emailConfirmations = context.EmailConfirmations
                .Take(2)
                .ToList();

            // Act
            var result = await sut.DeleteRangeAsync(emailConfirmations);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfDeleteRangeContainsUnknownEmailConfirmation()
        {
            // Arrange
            newEmailConfirmation.Id = 999;

            // Act
            var result = await sut.DeleteRangeAsync([newEmailConfirmation]);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfDeleteRangeReceivesInvalidEmailConfirmation()
        {
            // Arrange

            // Act
            var result = await sut.DeleteRangeAsync([newEmailConfirmation]);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Exception, Is.Not.Null);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfDeleteRangeReceivesNull()
        {
            // Arrange

            // Act
            var result = await sut.DeleteRangeAsync(null);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Exception, Is.Not.Null);
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
        public async Task ReturnFalseIfItDoesNotHaveAnEmailConfirmation()
        {
            // Arrange

            // Act
            var result = await sut.HasEntityAsync(999);

            // Assert
            Assert.That(result, Is.False);
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
        public async Task ReturnFalseIfItDoesNotHaveAnEmailConfirmationByUserAndAppId()
        {
            // Arrange
            var emailConfirmation = context.EmailConfirmations.FirstOrDefault();

            // Act
            var result = await sut.HasOutstandingEmailConfirmationAsync(
                emailConfirmation.UserId + 3,
                emailConfirmation.AppId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfRetrieveEmailConfirmationReceivesInvalidUserId()
        {
            // Arrange

            // Act
            var result = await sut.RetrieveEmailConfirmationAsync(0, 1);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Exception, Is.Not.Null);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfRetrieveEmailConfirmationReceivesInvalidAppId()
        {
            // Arrange

            // Act
            var result = await sut.RetrieveEmailConfirmationAsync(1, 0);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Exception, Is.Not.Null);
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
            Assert.That((EmailConfirmation)result.DomainObject, Is.InstanceOf<EmailConfirmation>());
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

        [Test, Category("Repository")]
        public void GetEmailConfirmationCount()
        {
            // Arrange
            var expectedCount = context.EmailConfirmations.Count();

            // Act
            var result = sut.GetCount();
            var actualCount = System.Convert.ToInt32(
                result.Object.GetType().GetProperty("TotalCount").GetValue(result.Object));

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(actualCount, Is.EqualTo(expectedCount));
        }

        [Test, Category("Repository")]
        public void ReturnFalseIfGetEmailConfirmationCountFails()
        {
            // Arrange
            context.Dispose();

            // Act
            var result = sut.GetCount();

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Exception, Is.Not.Null);
        }
    }
}

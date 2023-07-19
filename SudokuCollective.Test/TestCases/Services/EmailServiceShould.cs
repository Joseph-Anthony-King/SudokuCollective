using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Services;
using SudokuCollective.Test.Repositories;
using SudokuCollective.Test.Services;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Services
{
    public class EmailServiceShould
    {
        private DatabaseContext context;
        private EmailMetaData emailMetaData;
        private EmailMetaData incorrectEmailMetaData;
        private MockedRequestService mockedRequestService;
        private MockedAppsRepository mockedAppsRepository;
        private Mock<ILogger<EmailService>> mockedLogger;
        private IEmailService sut;
        private IEmailService sutFailure;
        private string toEmail;
        private string subject;
        private string html;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();

            emailMetaData = TestObjects.GetEmailMetaData();
            incorrectEmailMetaData = TestObjects.GetIncorrectEmailMetaData();
            mockedRequestService = new MockedRequestService();
            mockedAppsRepository = new MockedAppsRepository(context);
            mockedLogger = new Mock<ILogger<EmailService>>();

            sut = new EmailService(
                emailMetaData, 
                mockedRequestService.SuccessfulRequest.Object,
                mockedAppsRepository.SuccessfulRequest.Object,
                mockedLogger.Object);
                
            sutFailure = new EmailService(
                incorrectEmailMetaData, 
                mockedRequestService.SuccessfulRequest.Object,
                mockedAppsRepository.SuccessfulRequest.Object,
                mockedLogger.Object);

            toEmail = "sudokucollective@gmail.com";
            subject = "testing123...";
            html = "<h1>Hello World!</h1>";
        }

        [Test, Category("Services")]
        public async Task SendEmails()
        {
            // Arrange
            var app = mockedAppsRepository.SuccessfulRequest.Object.GetAsync(1);

            // Act
            var result = await sut.SendAsync(toEmail, subject, html, app.Id);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Services")]
        public async Task ReturnFalseIfSendEmailsFails()
        {
            // Arrange
            var app = mockedAppsRepository.SuccessfulRequest.Object.GetAsync(1);

            // Act
            var result = await sutFailure.SendAsync(toEmail, subject, html, app.Id);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}

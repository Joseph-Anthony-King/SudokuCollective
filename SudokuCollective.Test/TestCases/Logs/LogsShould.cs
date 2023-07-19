using System;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Data.Services;
using SudokuCollective.Logs;
using SudokuCollective.Logs.Utilities;

namespace SudokuCollective.Test.TestCases.Logs
{
    public class LogsShould
    {
        private Mock<ILogger<AppsService>> mockedLogger;

        [SetUp]
        public void SetUp()
        {
            mockedLogger = new Mock<ILogger<AppsService>>();
        }

        [Test, Category("Logs")]
        public void LogInformation()
        {
            // Arrange
            
            // Act and Assert
            try
            {
                SudokuCollectiveLogger.LogInformation<AppsService>(
                    mockedLogger.Object,
                    LogsUtilities.GetServiceLogEventId(),
                    "log information");

                Assert.That(true);
            }
            catch
            {
                Assert.That(false);
            }
        }

        [Test, Category("Logs")]
        public void LogWarnings()
        {
            // Arrange
            
            // Act and Assert
            try
            {
                SudokuCollectiveLogger.LogWarning<AppsService>(
                    mockedLogger.Object,
                    LogsUtilities.GetServiceWarningEventId(),
                    "log information");

                Assert.That(true);
            }
            catch
            {
                Assert.That(false);
            }
        }

        [Test, Category("Logs")]
        public void LogErrors()
        {
            // Arrange
            
            // Act and Assert
            try
            {
                SudokuCollectiveLogger.LogError<AppsService>(
                    mockedLogger.Object,
                    LogsUtilities.GetServiceWarningEventId(),
                    "log information",
                    new Exception());

                Assert.That(true);
            }
            catch
            {
                Assert.That(false);
            }
        }
    }
}

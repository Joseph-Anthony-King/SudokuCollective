using System;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Services;
using SudokuCollective.Repos.Utilities;

namespace SudokuCollective.Test.TestCases.Utilities
{
    public class ReposUtilitiesShould
    {
        [Test, Category("Utilities")]
        public void ProcessExceptions()
        {
            // Arrange
            var requestService = new RequestService();
            var mockedLogger = new Mock<ILogger<AppsService>>();
            IRepositoryResponse result = new RepositoryResponse();
            result.IsSuccess = false;
            var exception = new Exception();

            try
            {
                // Act
                result = ReposUtilities.ProcessException<AppsService>(requestService, mockedLogger.Object, result, exception);
                
                // Assert
                Assert.That(result.IsSuccess, Is.False);
            }
            catch
            {
                Assert.That(false);
            }
        }
    }
}
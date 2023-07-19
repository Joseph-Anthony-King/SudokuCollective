using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SudokuCollective.Logs.Utilities;

namespace SudokuCollective.Test.TestCases.Utilities
{
    public class LogsUtilitiesShould
    {
        [Test, Category("Utilities")]
        public void ObtainControllerLogEventId()
        {
            // Arrange
            
            // Act
            var result = LogsUtilities.GetControllerLogEventId();

            // Assert
            Assert.That(result, Is.TypeOf<EventId>());
            Assert.That(result.Id, Is.EqualTo(100));
            Assert.That(result.Name, Is.EqualTo("Controller Event"));
        }

        [Test, Category("Utilities")]
        public void ObtainControllerWarningEventId()
        {
            // Arrange
            
            // Act
            var result = LogsUtilities.GetControllerWarningEventId();

            // Assert
            Assert.That(result, Is.TypeOf<EventId>());
            Assert.That(result.Id, Is.EqualTo(101));
            Assert.That(result.Name, Is.EqualTo("Controller Event Warning"));
        }

        [Test, Category("Utilities")]
        public void ObtainControllerErrorEventId()
        {
            // Arrange
            
            // Act
            var result = LogsUtilities.GetControllerErrorEventId();

            // Assert
            Assert.That(result, Is.TypeOf<EventId>());
            Assert.That(result.Id, Is.EqualTo(102));
            Assert.That(result.Name, Is.EqualTo("Controller Event Error"));
        }

        [Test, Category("Utilities")]
        public void ObtainServiceLogEventId()
        {
            // Arrange
            
            // Act
            var result = LogsUtilities.GetServiceLogEventId();

            // Assert
            Assert.That(result, Is.TypeOf<EventId>());
            Assert.That(result.Id, Is.EqualTo(200));
            Assert.That(result.Name, Is.EqualTo("Service Event"));
        }

        [Test, Category("Utilities")]
        public void ObtainServiceWarningEventId()
        {
            // Arrange
            
            // Act
            var result = LogsUtilities.GetServiceWarningEventId();

            // Assert
            Assert.That(result, Is.TypeOf<EventId>());
            Assert.That(result.Id, Is.EqualTo(201));
            Assert.That(result.Name, Is.EqualTo("Service Event Warning"));
        }

        [Test, Category("Utilities")]
        public void ObtainServiceErrorEventId()
        {
            // Arrange
            
            // Act
            var result = LogsUtilities.GetServiceErrorEventId();

            // Assert
            Assert.That(result, Is.TypeOf<EventId>());
            Assert.That(result.Id, Is.EqualTo(202));
            Assert.That(result.Name, Is.EqualTo("Service Event Error"));
        }

        [Test, Category("Utilities")]
        public void ObtainSMTPLogEventId()
        {
            // Arrange
            
            // Act
            var result = LogsUtilities.GetSMTPEventId();

            // Assert
            Assert.That(result, Is.TypeOf<EventId>());
            Assert.That(result.Id, Is.EqualTo(203));
            Assert.That(result.Name, Is.EqualTo("SMTP Event"));
        }

        [Test, Category("Utilities")]
        public void ObtainRepositoryLogEventId()
        {
            // Arrange
            
            // Act
            var result = LogsUtilities.GetRepoLogEventId();

            // Assert
            Assert.That(result, Is.TypeOf<EventId>());
            Assert.That(result.Id, Is.EqualTo(300));
            Assert.That(result.Name, Is.EqualTo("Repository Event"));
        }

        [Test, Category("Utilities")]
        public void ObtainRepositoryWarningEventId()
        {
            // Arrange
            
            // Act
            var result = LogsUtilities.GetRepoWarningEventId();

            // Assert
            Assert.That(result, Is.TypeOf<EventId>());
            Assert.That(result.Id, Is.EqualTo(301));
            Assert.That(result.Name, Is.EqualTo("Repository Event Warning"));
        }

        [Test, Category("Utilities")]
        public void ObtainRepositoryErrorEventId()
        {
            // Arrange
            
            // Act
            var result = LogsUtilities.GetRepoErrorEventId();

            // Assert
            Assert.That(result, Is.TypeOf<EventId>());
            Assert.That(result.Id, Is.EqualTo(302));
            Assert.That(result.Name, Is.EqualTo("Repository Event Error"));
        }
    }
}

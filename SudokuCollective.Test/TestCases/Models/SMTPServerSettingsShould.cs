using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Test.TestCases.Models
{
    public class SMTPServerSettingsShould
    {
        private ISMTPServerSettings sut;
        
        [SetUp]
        public void Setup()
        {
            sut = new SMTPServerSettings();
        }

        [Test, Category("Models")]
        public void ImplementIDomainEntity()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut, Is.InstanceOf<IDomainEntity>());
        }

        [Test, Category("Models")]
        public void HaveAnID()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Id, Is.TypeOf<int>());
            Assert.That(sut.Id, Is.EqualTo(0));
        }
        
        [Test, Category("Models")]
        public void HaveExpectedProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.SmtpServer, Is.TypeOf<string>());
            Assert.That(sut.Port, Is.TypeOf<int>());
            Assert.That(sut.UserName, Is.TypeOf<string>());
            Assert.That(sut.Password, Is.TypeOf<string>());
            Assert.That(sut.FromEmail, Is.TypeOf<string>());
            Assert.That(sut.AppId, Is.TypeOf<int>());
        }

        public void CanSanitizeSensitiveInformation()
        {
            // Arrange and Act
            sut.Sanitize();

            Assert.That(sut.Id, Is.EqualTo(0));
            Assert.That(string.IsNullOrEmpty(sut.Password), Is.True);
            Assert.That(sut.AppId, Is.EqualTo(0));
        }
    }
}

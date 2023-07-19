using NUnit.Framework;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Data.Models.Authentication;

namespace SudokuCollective.Test.TestCases.Authentication
{
    public class TokenManagementShould
    {
        private ITokenManagement sut;

        [SetUp]
        public void Setup()
        {
            sut = new TokenManagement();
        }

        [Test, Category("Authentication")]
        public void HasRequiredProperties()
        {
            // Arrange and Act
            
            // Assert
            Assert.That(sut.Secret, Is.InstanceOf<string>());
            Assert.That(sut.Issuer, Is.InstanceOf<string>());
            Assert.That(sut.Audience, Is.InstanceOf<string>());
        }
    }
}

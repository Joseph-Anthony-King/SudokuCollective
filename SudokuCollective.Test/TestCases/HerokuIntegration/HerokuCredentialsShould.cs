using NUnit.Framework;
using SudokuCollective.HerokuIntegration.Models.Requests;

namespace SudokuCollective.Test.TestCases.HerokuIntegration
{
    public class HerokuCredentialsShould
    {
        private HerokuCredentials sut;

        [SetUp]
        public void Setup()
        {
            sut = new HerokuCredentials();
        }

        [Test, Category("HerokuIntegration")]
        public void HaveExpectedProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Client, Is.TypeOf<string>());
            Assert.That(sut.ID, Is.TypeOf<string>());
            Assert.That(sut.Description, Is.TypeOf<string>());
            Assert.That(sut.Scope, Is.TypeOf<string>());
            Assert.That(sut.Token, Is.TypeOf<string>());
            Assert.That(sut.UpdatedAt, Is.TypeOf<string>());
        }
    }
}

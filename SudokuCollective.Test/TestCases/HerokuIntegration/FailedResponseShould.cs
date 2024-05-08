using NUnit.Framework;
using SudokuCollective.HerokuIntegration.Models.Configuration;

namespace SudokuCollective.Test.TestCases.HerokuIntegration
{
    public class FailedResponseShould
    {
        private FailedResponse sut;

        [SetUp]
        public void Setup()
        {
            sut = new FailedResponse();
        }

        [Test, Category("HerokuIntegration")]
        public void HaveExpectedProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Id, Is.TypeOf<string>());
            Assert.That(sut.Message, Is.TypeOf<string>());
            Assert.That(sut.Url, Is.TypeOf<string>());
        }
    }
}

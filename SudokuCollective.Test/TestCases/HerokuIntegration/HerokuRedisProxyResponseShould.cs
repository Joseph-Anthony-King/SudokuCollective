using NUnit.Framework;
using SudokuCollective.HerokuIntegration.Models.Responses;

namespace SudokuCollective.Test.TestCases.HerokuIntegration
{
    public class HerokuRedisProxyResponseShould
    {
        private HerokuRedisProxyResponse sut;

        [SetUp]
        public void Setup()
        {
            sut = new HerokuRedisProxyResponse();
        }

        [Test, Category("HerokuIntegration")]
        public void HaveExpectedProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.IsSuccessful, Is.TypeOf<bool>());
            Assert.That(sut.Message, Is.TypeOf<string>());
        }
    }
}

using NUnit.Framework;
using SudokuCollective.HerokuIntegration.Models.Requests;

namespace SudokuCollective.Test.TestCases.HerokuIntegration
{
    public class HerokuRedisConnectionStringsShould
    {
        private HerokuRedisConnectionStrings sut;

        [SetUp]
        public void Setup()
        {
            sut = new HerokuRedisConnectionStrings();
        }

        [Test, Category("HerokuIntegration")]
        public void HaveExpectedProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.RedisUrl, Is.TypeOf<string>());
        }
    }
}

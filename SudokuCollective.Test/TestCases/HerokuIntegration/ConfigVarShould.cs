using NUnit.Framework;
using SudokuCollective.HerokuIntegration.Models.Configuration;

namespace SudokuCollective.Test.TestCases.HerokuIntegration
{
    public class ConfigVarShould
    {
        private ConfigVar sut;

        [SetUp]
        public void Setup()
        {
            sut = new ConfigVar();
        }

        [Test, Category("HerokuIntegration")]
        public void HaveExpectedProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Name, Is.TypeOf<string>());
            Assert.That(sut.Value, Is.TypeOf<string>());
        }
    }
}

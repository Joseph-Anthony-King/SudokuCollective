using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Data.Models.Results;

namespace SudokuCollective.Test.TestCases.Results
{
    public class EmailConfirmationSentResultShould
    {
        private IEmailConfirmationSentResult sut;

        [SetUp]
        public void Setup()
        {
            sut = new EmailConfirmationSentResult();
        }

        [Test, Category("Results")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.EmailConfirmationSent, Is.InstanceOf<bool?>());
        }

        [Test, Category("Results")]
        public void HaveADefaultConstructor()
        {
            // Arrange and Act
            sut = new EmailConfirmationSentResult();

            // Assert
            Assert.That(sut, Is.InstanceOf<EmailConfirmationSentResult>());
        }

        [Test, Category("Results")]
        public void HaveAConstructorThatAcceptsParams()
        {
            // Arrange and Act
            sut = new EmailConfirmationSentResult(true);

            // Assert
            Assert.That(sut, Is.InstanceOf<EmailConfirmationSentResult>());
        }
    }
}

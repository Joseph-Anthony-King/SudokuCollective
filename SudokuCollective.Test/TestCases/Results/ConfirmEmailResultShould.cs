using System;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Data.Models.Results;

namespace SudokuCollective.Test.TestCases.Results
{
    public class ConfirmEmailResultShould
    {
        private IConfirmEmailResult sut;

        [SetUp]
        public void Setup()
        {
            sut = new ConfirmEmailResult();
        }

        [Test, Category("Results")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.UserName, Is.InstanceOf<string>());
            Assert.That(sut.Email, Is.InstanceOf<string>());
            Assert.That(sut.DateUpdated, Is.InstanceOf<DateTime>());
            Assert.That(sut.AppTitle, Is.InstanceOf<string>());
            Assert.That(sut.AppUrl, Is.InstanceOf<string>());
            Assert.That(sut.IsUpdate, Is.Null);
            Assert.That(sut.NewEmailAddressConfirmed, Is.Null);
            Assert.That(sut.ConfirmationEmailSuccessfullySent, Is.Null);
        }

        [Test, Category("Results")]
        public void HaveADefaultConstructor()
        {
            // Arrange and Act
            sut = new ConfirmEmailResult();

            // Assert
            Assert.That(sut, Is.InstanceOf<ConfirmEmailResult>());
        }

        [Test, Category("Results")]
        public void HaveAConstructorThatAcceptsAUserName()
        {
            // Arrange

            // Act
            sut = new ConfirmEmailResult(
                "userName",
                "email",
                DateTime.Now,
                "appTitle",
                "url",
                false,
                false,
                false);

            // Assert
            Assert.That(sut, Is.InstanceOf<ConfirmEmailResult>());
        }
    }
}

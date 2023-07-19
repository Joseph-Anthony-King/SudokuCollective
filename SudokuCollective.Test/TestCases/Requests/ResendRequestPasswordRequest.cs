using NUnit.Framework;
using SudokuCollective.Data.Models.Requests;

namespace SudokuCollective.Test.TestCases.Requests
{
    public class ResendRequestPasswordRequestShould
    {
        private ResendRequestPasswordRequest sut;

        [SetUp]
        public void Setup()
        {
            sut = new ResendRequestPasswordRequest();
        }

        [Test, Category("Requests")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.UserId, Is.InstanceOf<int>());
            Assert.That(sut.AppId, Is.InstanceOf<int>());
        }

        [Test, Category("Requests")]
        public void HasADefaultConstructor()
        {
            // Arrange and Act
            sut = new ResendRequestPasswordRequest();

            // Assert
            Assert.That(sut, Is.InstanceOf<ResendRequestPasswordRequest>());
        }

        [Test, Category("Requests")]
        public void HasAConstructorThatAcceptsParams()
        {
            // Arrange and Act
            sut = new ResendRequestPasswordRequest(1, 1);

            // Assert
            Assert.That(sut, Is.InstanceOf<ResendRequestPasswordRequest>());
        }
    }
}

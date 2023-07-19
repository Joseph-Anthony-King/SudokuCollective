using System.Text.Json;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Data.Models.Params;

namespace SudokuCollective.Test.TestCases.Params
{
    public class RequestShould
    {
        private IRequest sut;

        [SetUp]
        public void Setup()
        {
            sut = new Request();
        }

        [Test, Category("Params")]
        public void HaveADefaultConstructor()
        {
            // Arrange and Act
            sut = new Request();

            // Assert
            Assert.That(sut, Is.InstanceOf<Request>());
        }

        [Test, Category("Params")]
        public void HasRequiredProperties()
        {
            // Arrange and Act
            
            // Assert
            Assert.That(sut.License, Is.InstanceOf<string>());
            Assert.That(sut.RequestorId, Is.InstanceOf<int>());
            Assert.That(sut.AppId, Is.InstanceOf<int>());
            Assert.That(sut.Paginator, Is.InstanceOf<IPaginator>());
        }

        [Test, Category("Params")]
        public void AcceptsJsonElementsInTheDataPacket()
        {
            // Arrange
            var json = new JsonElement();

            // Act
            sut.Payload = json;
            
            // Assert
            Assert.That(sut.Payload, Is.InstanceOf<JsonElement>());
        }
    }
}

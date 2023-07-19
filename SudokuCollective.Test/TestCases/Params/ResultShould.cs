using System.Collections.Generic;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Data.Models.Params;

namespace SudokuCollective.Test.TestCases.Params
{
    public class ResultShould
    {
        private IResult sut;

        [SetUp]
        public void Setup()
        {
            sut = new Result();
        }

        [Test, Category("Params")]
        public void HaveADefaultConstructor()
        {
            // Arrange and Act
            sut = new Result();

            // Assert
            Assert.That(sut, Is.InstanceOf<Result>());
        }

        [Test, Category("Params")]
        public void HasRequiredProperties()
        {
            // Arrange and Act
            
            // Assert
            Assert.That(sut.IsSuccess, Is.InstanceOf<bool>());
            Assert.That(sut.IsFromCache, Is.InstanceOf<bool>());
            Assert.That(sut.Message, Is.InstanceOf<string>());
            Assert.That(sut.Payload, Is.InstanceOf<List<object>>());
        }
    }
}

using System;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Test.TestCases.Models
{
    public class GalleryAppShould
    {
        private IGalleryApp sut;

        [SetUp]
        public void Setup()
        {
            sut = new GalleryApp();
        }

        [Test, Category("Models")]
        public void ImplementIDomainEntity()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut, Is.InstanceOf<IDomainEntity>());
        }

        [Test, Category("Models")]
        public void HaveExpectedProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Id, Is.TypeOf<int>());
            Assert.That(sut.Name, Is.TypeOf<string>());
            Assert.That(sut.Url, Is.TypeOf<string>());
            Assert.That(sut.CreatedBy, Is.TypeOf<string>());
            Assert.That(sut.DateCreated, Is.TypeOf<DateTime>());
            Assert.That(sut.DateUpdated, Is.TypeOf<DateTime>());
        }

        [Test, Category("Models")]
        public void NullifySourceCodeUrl()
        {
            // Arrange and Act
            sut.NullifySourceCodeUrl();

            // Assert
            Assert.That(sut.SourceCodeUrl, Is.Null);
        }
    }
}
using NUnit.Framework;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Params
{
    public class PaginatorShould
    {
        private IPaginator sut;

        [SetUp]
        public void Setup()
        {
            sut = TestObjects.GetPaginator();
        }

        [Test, Category("Params")]
        public void HasRequiredProperties()
        {
            // Arrange and Act
            
            // Assert
            Assert.That(sut.Page, Is.InstanceOf<int>());
            Assert.That(sut.ItemsPerPage, Is.InstanceOf<int>());
            Assert.That(sut.SortBy, Is.InstanceOf<SortValue>());
            Assert.That(sut.OrderByDescending, Is.InstanceOf<bool>());
            Assert.That(sut.IncludeCompletedGames, Is.InstanceOf<bool>());
        }

        [Test, Category("Params")]
        public void HasADefaultConstructor()
        {
            // Arrange and Act
            sut = new Paginator();

            // Assert
            Assert.That(sut, Is.InstanceOf<Paginator>());
        }

        [Test, Category("Params")]
        public void HasAConstructorThatTakesParams()
        {
            // Arrange and Act
            sut = new Paginator(1, 10, 1, false, false);

            // Assert
            Assert.That(sut, Is.InstanceOf<Paginator>());
        }

        [Test, Category("Params")]
        public void HasAConstructorThatTakesSortValue()
        {
            // Arrange and Act
            sut = new Paginator(1, 10, SortValue.ID, false, false);

            // Assert
            Assert.That(sut, Is.InstanceOf<Paginator>());
        }

        [Test, Category("Params")]
        public void IsNullReturnsFalseIfSortValueIsSet()
        {
            // Arrange
            sut.SortBy = SortValue.ID;

            // Act
            var result = sut.IsNull();
            
            // Assert
            Assert.That(sut.SortBy, Is.Not.EqualTo(SortValue.NULL));
            Assert.That(result, Is.False);
        }

        [Test, Category("Params")]
        public void IsNullReturnsTrueIfSortValueIsNotSet()
        {
            // Arrange
            sut = new Paginator();

            // Act
            var result = sut.IsNull();
            
            // Assert
            Assert.That(sut.SortBy, Is.EqualTo(SortValue.NULL));
            Assert.That(result, Is.True);
        }
    }
}

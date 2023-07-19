using NUnit.Framework;
using SudokuCollective.Core.Enums;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Validation.Attributes;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Attributes
{
    public class PaginatorValidatedAttributeShould
    {
        private PaginatorValidatedAttribute sut;

        [SetUp]
        public void Setup()
        {
            sut = new PaginatorValidatedAttribute();
        }

        [Test, Category("Attributes")]
        public void AcceptsValidPaginators()
        {
            // Arrange
            var paginator = TestObjects.GetPaginator();

            // Act
            var result = sut.IsValid(paginator);

            // Assert
            Assert.That(paginator, Is.InstanceOf<Paginator>());
            Assert.That(result, Is.True);
        }

        [Test, Category("Attributes")]
        public void RejectsPaginatorsWithSortValuesHigherThanFifteen()
        {
            // Arrange

            // Score is the highest sort value, we add 1 to create an invalid paginator
            var paginator = new Paginator()
            {
                Page = 1,
                ItemsPerPage = 10,
                SortBy = SortValue.URL + 1,
                OrderByDescending = false,
                IncludeCompletedGames = false
            };

            // Act
            var result = sut.IsValid(paginator);

            // Assert
            Assert.That(paginator, Is.InstanceOf<Paginator>());
            Assert.That(result, Is.False);
        }

        [Test, Category("Attributes")]
        public void RejectsPaginatorsWithSortValuesLessThanZero()
        {
            // Arrange

            // Null is the lowest sort value, we subtract 1 to create an invalid paginator
            var paginator = new Paginator()
            {
                Page = 1,
                ItemsPerPage = 10,
                SortBy = SortValue.NULL - 1,
                OrderByDescending = false,
                IncludeCompletedGames = false
            };

            // Act
            var result = sut.IsValid(paginator);

            // Assert
            Assert.That(paginator, Is.InstanceOf<Paginator>());
            Assert.That(result, Is.False);
        }
    }
}

using System.Linq;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Test.TestCases.Models
{
    public class SudokuCellShould
    {
        private ISudokuCell sut;

        [SetUp]
        public void Setup()
        {
            sut = new SudokuCell();
        }

        [Test, Category("Models")]
        public void ImplementIDomainEntity()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut, Is.InstanceOf<IDomainEntity>());
        }

        [Test, Category("Models")]
        public void HaveAnID()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Id, Is.TypeOf<int>());
            Assert.That(sut.Id, Is.EqualTo(0));
        }

        [Test, Category("Models")]
        public void HaveAnAssociatedMatrix()
        {
            // Arrange and Act
            var testMatrix = new SudokuMatrix();
            sut = testMatrix.SudokuCells[0];

            // Assert
            Assert.That(sut.SudokuMatrixId, Is.TypeOf<int>());
            Assert.That(testMatrix.Id, Is.EqualTo(sut.SudokuMatrixId));
            Assert.That(testMatrix, Is.TypeOf<SudokuMatrix>());
        }

        [Test, Category("Models")]
        public void SetCoordinatesInConstructor()
        {
            // Arrange
            var index = 1;
            var column = 1;
            var region = 1;
            var row = 1;
            var matrixId = 1;

            // Act
            sut = new SudokuCell(index, column, region, row, matrixId);

            // Assert
            Assert.That(sut.Index, Is.EqualTo(1));
            Assert.That(sut.Column, Is.EqualTo(1));
            Assert.That(sut.Region, Is.EqualTo(1));
            Assert.That(sut.Row, Is.EqualTo(1));
        }

        [Test, Category("Models")]
        public void HaveADefaultValueOfZero()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Value, Is.EqualTo(0));
        }

        [Test, Category("Models")]
        public void BeObscuredByDefault()
        {
            // Arrange and Act
            sut.Value = 9;

            // Assert
            Assert.That(sut.Hidden, Is.EqualTo(true));
            Assert.That(sut.DisplayedValue, Is.EqualTo(0));
            Assert.That(sut.DisplayedValue, Is.Not.EqualTo(sut.Value));
        }

        [Test, Category("Models")]
        public void BeVisibleIfObscuredIsFalse()
        {
            // Arrange
            var setValue = 9;

            sut.Value = setValue;

            // Act
            sut.Hidden = false;

            // Assert
            Assert.That(setValue, Is.Not.EqualTo(0));
            Assert.That(sut.DisplayedValue, Is.EqualTo(setValue));
            Assert.That(sut.DisplayedValue, Is.EqualTo(sut.Value));
        }

        [Test, Category("Models")]
        public void HaveAvailableValuesNineCountIfValueIsZero()
        {
            // Arrange and Act
            var testMatrix = new SudokuMatrix();
            sut = testMatrix.SudokuCells[0];

            // Assert
            Assert.That(sut.AvailableValues.Count, Is.EqualTo(9));
        }

        [Test, Category("Models")]
        public void HaveAvailableValuesZeroCountIfValueNonZero()
        {
            // Arrange and Act
            sut.Value = 9;

            // Assert
            Assert.That(sut.AvailableValues.Where(a => a.Available).Count, Is.EqualTo(0));
        }

        [Test, Category("Models")]
        public void HaveToInt32OutputValueAsInt()
        {
            // Arrange and Act
            var setValue = 9;
            sut.Value = setValue;
            sut.Hidden = false;

            // Assert
            Assert.That(sut.ToInt32(), Is.TypeOf<int>());
            Assert.That(sut.ToInt32(), Is.EqualTo(setValue));
        }

        [Test, Category("Models")]
        public void HaveToStringOutputValueAsString()
        {
            // Arrange and Act
            var setValue = 9;
            sut.Value = setValue;
            sut.Hidden = false;

            // Assert
            Assert.That(sut.ToString(), Is.TypeOf<string>());
            Assert.That(sut.ToString(), Is.EqualTo(setValue.ToString()));
        }
    }
}

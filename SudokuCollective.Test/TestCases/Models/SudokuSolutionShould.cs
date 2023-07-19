using System.Collections.Generic;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Test.TestCases.Models
{
    public class SudokuSolutionShould
    {
        private ISudokuSolution sut;

        [SetUp]
        public void Setup()
        {
            sut = new SudokuSolution();
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
        public void HasNineRowsWithNineCells()
        {
            // Arrange and Act

            // Assert
            Assert.That(((SudokuSolution)sut).FirstRow, Is.InstanceOf<List<int>>());
            Assert.That(((SudokuSolution)sut).FirstRow.Count, Is.EqualTo(9));
            Assert.That(((SudokuSolution)sut).SecondRow, Is.InstanceOf<List<int>>());
            Assert.That(((SudokuSolution)sut).SecondRow.Count, Is.EqualTo(9));
            Assert.That(((SudokuSolution)sut).ThirdRow, Is.InstanceOf<List<int>>());
            Assert.That(((SudokuSolution)sut).ThirdRow.Count, Is.EqualTo(9));
            Assert.That(((SudokuSolution)sut).FourthRow, Is.InstanceOf<List<int>>());
            Assert.That(((SudokuSolution)sut).FourthRow.Count, Is.EqualTo(9));
            Assert.That(((SudokuSolution)sut).FifthRow, Is.InstanceOf<List<int>>());
            Assert.That(((SudokuSolution)sut).FifthRow.Count, Is.EqualTo(9));
            Assert.That(((SudokuSolution)sut).SixthRow, Is.InstanceOf<List<int>>());
            Assert.That(((SudokuSolution)sut).SixthRow.Count, Is.EqualTo(9));
            Assert.That(((SudokuSolution)sut).SeventhRow, Is.InstanceOf<List<int>>());
            Assert.That(((SudokuSolution)sut).SeventhRow.Count, Is.EqualTo(9));
            Assert.That(((SudokuSolution)sut).EighthRow, Is.InstanceOf<List<int>>());
            Assert.That(((SudokuSolution)sut).EighthRow.Count, Is.EqualTo(9));
            Assert.That(((SudokuSolution)sut).NinthRow, Is.InstanceOf<List<int>>());
            Assert.That(((SudokuSolution)sut).NinthRow.Count, Is.EqualTo(9));
        }

        [Test, Category("Models")]
        public void AcceptsReferenceToGameObjects()
        {
            // Arrange
            var game = new Game();

            // Act
            sut.Game = game;

            // Assert
            Assert.That(game, Is.InstanceOf<Game>());
            Assert.That(((SudokuSolution)sut).Game, Is.InstanceOf<Game>());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using Moq;
using NUnit.Framework;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Test.TestCases.Models
{
    public class GameShould
    {
        private IGame sut;

        [SetUp]
        public void Setup()
        {
            InitializeSetup(out sut);
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
        public void HaveAWorkingConstructor()
        {
            // Arrange and Act

            // Assert
            Assert.IsNotNull(sut);
        }

        [Test, Category("Models")]
        public void HaveAConstructorThatAccceptsOnlyDifficultyAndIntList()
        {
            // Arrange
            var difficulty = new Difficulty { DifficultyLevel = DifficultyLevel.TEST };

            var intList = new List<int> {
                2, 9, 8, 1, 3, 4, 6, 7, 5,
                3, 1, 6, 5, 8, 7, 2, 9, 4,
                4, 5, 7, 6, 9, 2, 1, 8, 3,
                9, 7, 1, 2, 4, 3, 5, 6, 8,
                5, 8, 3, 7, 6, 1, 4, 2, 9,
                6, 2, 4, 9, 5, 8, 3, 1, 7,
                7, 3, 5, 8, 2, 6, 9, 4, 1,
                8, 4, 2, 3, 1, 9, 7, 5, 6,
                1, 6, 9, 4, 7, 5, 8, 3, 2 };

            // Act
            sut = new Game(difficulty, intList);

            // Assert
            Assert.IsNotNull(sut);
            Assert.That(sut, Is.InstanceOf<Game>());
        }

        [Test, Category("Models")]
        public void HaveAnAssociatedMatrix()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.SudokuMatrix, Is.InstanceOf<SudokuMatrix>());
            Assert.That(sut.SudokuMatrixId, Is.InstanceOf<int>());
            Assert.That(sut.SudokuMatrixId, Is.EqualTo(sut.SudokuMatrix.Id));
        }

        [Test, Category("Models")]
        public void HaveAnAssociatedSolution()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.SudokuSolution, Is.TypeOf<SudokuSolution>());
            Assert.That(sut.SudokuSolutionId, Is.InstanceOf<int>());
            Assert.That(sut.SudokuSolutionId, Is.EqualTo(sut.SudokuSolution.Id));
        }

        [Test, Category("Models")]
        public void HasAReferenceToTheHostingApp()
        {
            // Arrange and Act

            // Asser
            Assert.That(sut.AppId, Is.InstanceOf<int>());
        }

        [Test, Category("Models")]
        public void ContinueGameFieldDefaultsToTrue()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.ContinueGame, Is.InstanceOf<bool>());
            Assert.That(sut.ContinueGame, Is.True);
        }

        [Test, Category("Models")]
        public void ReturnTrueIfSolved()
        {
            // Arrange and Act
            sut.KeepScore = true;
            sut.SudokuMatrix.Difficulty.DifficultyLevel = DifficultyLevel.EVIL;
            foreach (var cell in sut.SudokuMatrix.SudokuCells)
            {
                cell.DisplayedValue = cell.Value;
            }
            sut.SudokuMatrix.Stopwatch.Start();
            Thread.Sleep(1000);
            sut.SudokuMatrix.Stopwatch.Stop();
            sut.IsSolved();

            // Assert
            Assert.That(sut.ContinueGame, Is.False);
            Assert.That(sut.Score, Is.GreaterThan(0));
            Assert.That(sut.TimeToSolve, Is.GreaterThan(new TimeSpan(0, 0, 0)));
            Assert.That(sut.DateCompleted, Is.GreaterThan(DateTime.MinValue));
            Assert.That(sut.DateUpdated, Is.GreaterThan(DateTime.MinValue));
            Assert.That(sut.DateUpdated, Is.EqualTo(sut.DateCompleted));
        }

        private static void InitializeSetup(out IGame sut)
        {
            var user = new Mock<User>();
            var matrix = new Mock<SudokuMatrix>();

            user.Setup(u => u.Games).Returns(new List<Game>());
            matrix.Setup(m => m.SudokuCells).Returns(new List<SudokuCell>());
            matrix.Setup(m => m.IsSolved()).Returns(true);

            sut = new Game(
                user.Object,
                matrix.Object,
                new Difficulty()
                {
                    Name = "Test",
                    DifficultyLevel = DifficultyLevel.TEST
                }
            );
        }
    }
}

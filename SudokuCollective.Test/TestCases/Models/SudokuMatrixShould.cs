using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Test.TestCases.Models
{
    public class SudokuMatrixShould
    {
        private string stringList;
        private ISudokuMatrix populatedTestMatrix;
        private ISudokuMatrix sut;

        [SetUp]
        public void Setup()
        {
            InitializeSetup(out stringList, out populatedTestMatrix, out sut);
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
        public void AcceptStringInConstructor()
        {
            // Arrange and Act
            sut = new SudokuMatrix(stringList);

            // Assert
            Assert.That(stringList, Is.TypeOf<string>());
            Assert.That(sut, Is.Not.Null);
            Assert.That(stringList.Length, Is.EqualTo(81));
            Assert.That(sut.SudokuCells.Count, Is.EqualTo(81));
        }

        [Test, Category("Models")]
        public void AcceptIntListInConstructor()
        {
            // Arrange and Act
            var intList = populatedTestMatrix.ToIntList();
            sut = new SudokuMatrix(intList);

            // Assert
            Assert.That(intList, Is.TypeOf<List<int>>());
            Assert.That(sut, Is.Not.Null);
            Assert.That(intList.Count, Is.EqualTo(81));
            Assert.That(sut.SudokuCells.Count, Is.EqualTo(81));
        }

        [Test, Category("Models")]
        public void CreateZeroedListWithBlankConstructor()
        {
            // Arrange and Act
            sut = new SudokuMatrix();

            // Assert
            Assert.That(((SudokuMatrix)sut).SudokuCells[0].Value, Is.EqualTo(0));
            Assert.That(((SudokuMatrix)sut).SudokuCells[9].Value, Is.EqualTo(0));
            Assert.That(((SudokuMatrix)sut).SudokuCells[18].Value, Is.EqualTo(0));
            Assert.That(((SudokuMatrix)sut).SudokuCells[27].Value, Is.EqualTo(0));
            Assert.That(((SudokuMatrix)sut).SudokuCells[36].Value, Is.EqualTo(0));
            Assert.That(((SudokuMatrix)sut).SudokuCells[45].Value, Is.EqualTo(0));
            Assert.That(((SudokuMatrix)sut).SudokuCells[54].Value, Is.EqualTo(0));
            Assert.That(((SudokuMatrix)sut).SudokuCells[63].Value, Is.EqualTo(0));
            Assert.That(((SudokuMatrix)sut).SudokuCells[72].Value, Is.EqualTo(0));
        }

        [Test, Category("Models")]
        public void AcceptDifficultyAndIntListConstructor()
        {
            // Arrange
            var difficulty = new Difficulty { DifficultyLevel = DifficultyLevel.TEST };
            var intList = populatedTestMatrix.ToIntList();

            // Act
            sut = new SudokuMatrix(difficulty, intList);

            // Assert
            Assert.That(intList, Is.TypeOf<List<int>>());
            Assert.That(difficulty, Is.TypeOf<Difficulty>());
            Assert.That(sut.Difficulty, Is.TypeOf<Difficulty>());
            Assert.That(sut, Is.TypeOf<SudokuMatrix>());
            Assert.That(sut.SudokuCells.Count, Is.EqualTo(81));
        }

        [Test, Category("Models")]
        public void ReturnTrueIfValid()
        {
            // Arrange and Act
            sut = populatedTestMatrix;

            // Assert
            Assert.That(sut.IsValid(), Is.True);
        }

        [Test, Category("Models")]
        public void OutputValuesAsIntListWithToInt32List()
        {
            // Arrange
            sut = populatedTestMatrix;

            // Act
            var result = sut.ToIntList();

            // Assert
            Assert.That(result, Is.TypeOf<List<int>>());
            Assert.That(result.Count, Is.EqualTo(81));
        }

        [Test, Category("Models")]
        public void OutputValuesAsIntListWithToDisplayedValuesList()
        {
            // Arrange
            sut = populatedTestMatrix;

            // Act
            var result = sut.ToDisplayedIntList();

            // Assert
            Assert.That(result, Is.TypeOf<List<int>>());
            Assert.That(result.Count, Is.EqualTo(81));
        }

        [Test, Category("Models")]
        public void OutputValuesAsStringWithToString()
        {
            // Arrange
            sut = populatedTestMatrix;

            // Act
            var result = sut.ToString();

            // Assert
            Assert.That(result, Is.TypeOf<string>());
            Assert.That(result.Length, Is.EqualTo(81));
        }

        [Test, Category("Models")]
        public void HaveNoObscuredCellsOnTestDifficulty()
        {
            // Arrange
            populatedTestMatrix.SetDifficulty(new Difficulty()
            {
                Name = "Test",
                DifficultyLevel = DifficultyLevel.TEST
            });

            // Act
            sut = populatedTestMatrix;

            var result = 0;

            foreach (var cell in sut.SudokuCells)
            {
                if (cell.Hidden == false)
                {
                    result++;
                }
            }

            // Assert
            Assert.That(result, Is.EqualTo(81));
        }

        [Test, Category("Models")]
        public void HaveHiddenCellsIfDifficultyIsNotTest()
        {
            // Arrange
            populatedTestMatrix.SetDifficulty(new Difficulty()
            {
                Name = "Easy",
                DifficultyLevel = DifficultyLevel.EASY
            });

            // Act
            sut = populatedTestMatrix;

            var result = false;

            foreach (var cell in sut.SudokuCells)
            {
                if (cell.Hidden == false)
                {
                    result = true;
                }
            }

            // Assert
            Assert.That(result, Is.True); ;
        }

        [Test, Category("Models")]
        public void HaveNoHiddenCellsIfDifficultyIsTest()
        {
            // Arrange
            populatedTestMatrix.SetDifficulty(new Difficulty()
            {
                Name = "Test",
                DifficultyLevel = DifficultyLevel.TEST
            });

            // Act
            sut = populatedTestMatrix;

            var result = false;

            foreach (var cell in sut.SudokuCells)
            {
                if (cell.Hidden == true)
                {
                    result = true;
                }
            }

            // Assert
            Assert.That(result, Is.False);
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
            Assert.That(((SudokuMatrix)sut).Game, Is.InstanceOf<Game>());
        }

        [Test, Category("Models")]
        public void DetermineIfMatrixIsSolved()
        {
            // Arrange
            var intList = new List<int>() {
                    4, 1, 9, 2, 6, 5, 3, 8, 7,
                    2, 8, 3, 1, 7, 9, 4, 5, 6,
                    5, 6, 7, 4, 3, 8, 9, 1, 2,
                    1, 2, 5, 3, 9, 4, 7, 6, 8,
                    7, 3, 8, 5, 1, 6, 2, 4, 9,
                    6, 9, 4, 7, 8, 2, 5, 3, 1,
                    3, 5, 6, 8, 2, 7, 1, 9, 4,
                    8, 7, 1, 9, 4, 3, 6, 2, 5,
                    9, 4, 2, 6, 5, 1, 8, 7, 3
                };

            // Act
            sut = new SudokuMatrix(intList);

            sut.SetDifficulty(new Difficulty()
            {
                Name = "Test",
                DifficultyLevel = DifficultyLevel.TEST
            });

            // Assert
            Assert.That(sut.IsSolved(), Is.True);
        }

        [Test, Category("Models")]
        public void GenerateValidSolutions()
        {
            // Arrange
            sut = new SudokuMatrix();

            // Act
            sut.GenerateSolution();

            // Assert
            Assert.That(sut.IsValid(), Is.True);
        }

        [Test, Category("Models")]
        public async Task SolveSudokuMatrices()
        {
            // Arrange
            var intList = new List<int>() {
                    4, 1, 9, 2, 6, 5, 3, 8, 7,
                    2, 8, 3, 1, 7, 9, 4, 5, 6,
                    5, 6, 7, 4, 3, 8, 9, 1, 2,
                    1, 2, 5, 3, 9, 4, 7, 6, 8,
                    7, 3, 8, 5, 1, 6, 2, 4, 9,
                    6, 9, 4, 7, 8, 2, 5, 3, 1,
                    3, 5, 6, 8, 2, 7, 1, 9, 4,
                    8, 7, 1, 9, 4, 3, 6, 2, 5,
                    9, 4, 2, 6, 5, 1, 8, 7, 3
                };

            sut = new SudokuMatrix(intList);

            sut.SetDifficulty(new Difficulty()
            {
                Name = "Easy",
                DifficultyLevel = DifficultyLevel.EASY
            }
            );

            // Act
            await sut.Solve();

            // Assert
            Assert.That(sut.IsValid(), Is.EqualTo(true));
        }

        private static void InitializeSetup(
            out string stringList,
            out ISudokuMatrix populatedTestMatrix, 
            out ISudokuMatrix sut)
        {
            populatedTestMatrix = new SudokuMatrix();
            populatedTestMatrix.GenerateSolution();

            var sb = new StringBuilder();

            foreach (var i in populatedTestMatrix.ToIntList())
            {
                sb.Append(i);
            }

            stringList = sb.ToString();
            sut = new SudokuMatrix(stringList);
        }
    }
}

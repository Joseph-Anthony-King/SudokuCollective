using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Models
{
    public class Game : IGame
    {
        #region Fields
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
        #endregion

        #region Properties
        [Required, JsonPropertyName("id")]
        public int Id { get; set; }
        [Required, JsonPropertyName("userId")]
        public int UserId { get; set; }
        [Required, JsonPropertyName("sudokuMatrixId")]
        public int SudokuMatrixId { get; set; }
        [Required, JsonPropertyName("sudokuSolutionId")]
        public int SudokuSolutionId { get; set; }
        [Required, JsonPropertyName("appId")]
        public int AppId { get; set; }
        [Required, JsonPropertyName("continueGame")]
        public bool ContinueGame { get; set; }
        [Required, JsonPropertyName("score")]
        public int Score { get; set; }
        [Required, JsonPropertyName("keepScore")]
        public bool KeepScore { get; set; }
        [Required, JsonPropertyName("dateCreated")]
        public DateTime DateCreated { get; set; }
        [Required, JsonPropertyName("dateUpdated")]
        public DateTime DateUpdated { get; set; }
        [Required, JsonPropertyName("dateCompleted")]
        public DateTime DateCompleted { get; set; }
        [JsonIgnore]
        public TimeSpan TimeToSolve
        {
            get => GetTimeSpan();
        }
        [JsonIgnore]
        IUser IGame.User
        {
            get => User;
            set => User = (User)value;
        }
        [JsonIgnore]
        public User User { get; set; }
        [JsonIgnore]
        ISudokuMatrix IGame.SudokuMatrix
        {
            get => SudokuMatrix;
            set => SudokuMatrix = (SudokuMatrix)value;
        }
        [Required, JsonPropertyName("sudokuMatrix")]
        public virtual SudokuMatrix SudokuMatrix { get; set; }
        [JsonIgnore]
        ISudokuSolution IGame.SudokuSolution
        {
            get => SudokuSolution;
            set => SudokuSolution = (SudokuSolution)value;
        }
        [Required, JsonPropertyName("sudokuSolution")]
        public virtual SudokuSolution SudokuSolution { get; set; }
        #endregion

        #region Constructors
        public Game()
        {
            Id = 0;
            DateCreated = DateTime.UtcNow;
            DateUpdated = DateTime.MinValue;
            DateCompleted = DateTime.MinValue;
            ContinueGame = true;
            Score = 0;
            KeepScore = false;
            SudokuMatrix = new SudokuMatrix
            {
                Game = this
            };
            SudokuSolution = new SudokuSolution
            {
                Game = this
            };
            AppId = 0;
        }

        public Game(
            User user,
            SudokuMatrix matrix,
            IDifficulty difficulty,
            int appId = 0) : this()
        {
            User = user;
            SudokuMatrix = matrix;
            SudokuMatrix.Difficulty = (Difficulty)difficulty;
            AppId = appId;

            User.Games.Add(this);
        }

        public Game(IDifficulty difficulty, List<int> intList = null) : this()
        {
            if (intList != null)
            {
                SudokuMatrix = new SudokuMatrix(difficulty, intList);
            }
            else
            {
                SudokuMatrix.Difficulty = (Difficulty)difficulty;
            }
        }

        [JsonConstructor]
        public Game(
            int id,
            int userId,
            int sudokuMatrixId,
            int sudokuSolutionId,
            int appId,
            bool continueGame,
            int score,
            bool keepScore,
            DateTime dateCreated,
            DateTime dateUpdated,
            DateTime dateCompleted,
            SudokuMatrix sudokuMatrix = null,
            SudokuSolution sudokuSolution = null)
        {
            Id = id;
            UserId = userId;
            SudokuMatrixId = sudokuMatrixId;
            SudokuSolutionId = sudokuSolutionId;
            AppId = appId;
            ContinueGame = continueGame;
            Score = score;
            KeepScore = keepScore;
            DateCreated = dateCreated;
            DateUpdated = dateUpdated;
            DateCompleted = dateCompleted;

            if (sudokuMatrix != null)
            {
                SudokuMatrix = sudokuMatrix;
            }

            if (sudokuSolution != null)
            {
                SudokuSolution = sudokuSolution;
            }
        }
        #endregion

        #region Methods
        public bool IsSolved()
        {
            if (ContinueGame)
            {
                if (SudokuMatrix.IsSolved())
                {
                    if (SudokuMatrix.Stopwatch.IsRunning)
                    {
                        SudokuMatrix.Stopwatch.Stop();
                    }

                    foreach (var sudokuCell in SudokuMatrix.SudokuCells)
                    {
                        if (sudokuCell.DisplayedValue > 0)
                        {
                            sudokuCell.Value = sudokuCell.DisplayedValue;
                        }
                    }

                    var solvedDate = DateTime.UtcNow;

                    DateUpdated = solvedDate;
                    DateCompleted = solvedDate;

                    if (KeepScore)
                    {
                        var maxTicks = 144000000000;

                        if (TimeToSolve.Ticks < maxTicks)
                        {
                            if (SudokuMatrix.Difficulty.DifficultyLevel == DifficultyLevel.EASY)
                            {
                                Score = Convert.ToInt32(((maxTicks - TimeToSolve.Ticks) / 50000000) * .2);
                            }
                            else if (SudokuMatrix.Difficulty.DifficultyLevel == DifficultyLevel.MEDIUM)
                            {
                                Score = Convert.ToInt32(((maxTicks - TimeToSolve.Ticks) / 50000000) * .4);
                            }
                            else if (SudokuMatrix.Difficulty.DifficultyLevel == DifficultyLevel.HARD)
                            {
                                Score = Convert.ToInt32(((maxTicks - TimeToSolve.Ticks) / 50000000) * .6);
                            }
                            else if (SudokuMatrix.Difficulty.DifficultyLevel == DifficultyLevel.EVIL)
                            {
                                Score = Convert.ToInt32(((maxTicks - TimeToSolve.Ticks) / 50000000) * .8);
                            }
                            else
                            {
                                Score = 0;
                            }

                            if (SudokuMatrix.Difficulty.DifficultyLevel != DifficultyLevel.NULL || SudokuMatrix.Difficulty.DifficultyLevel != DifficultyLevel.TEST)
                            {
                                if (Score < 1)
                                {
                                    Score = 1;
                                }
                            }
                        }
                        else
                        {
                            Score = 0;
                        }
                    }

                    ContinueGame = false;
                    SudokuSolution.SolutionList = SudokuMatrix.ToIntList();
                    SudokuSolution.DateSolved = solvedDate;
                }
            }

            return !ContinueGame;
        }

        public override string ToString() => string.Format(base.ToString() + ".Id:{0}.AppId:{1}.UserId:{2}", Id, AppId, UserId);

        public string ToJson() => JsonSerializer.Serialize(
            this,
            _serializerOptions);

        public IDomainEntity Cast<T>() => throw new System.NotImplementedException();

        private TimeSpan GetTimeSpan()
        {
            if (DateCompleted == DateTime.MinValue)
            {
                return TimeSpan.Zero;
            }
            else
            {
                return DateCompleted - DateCreated;
            }
        }
        #endregion
    }
}

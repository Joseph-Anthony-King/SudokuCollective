using System;

namespace SudokuCollective.Core.Interfaces.Models.DomainEntities
{
    public interface IGame : IDomainEntity
    {
        int UserId { get; set; }
        int SudokuMatrixId { get; set; }
        int SudokuSolutionId { get; set; }
        int AppId { get; set; }
        bool ContinueGame { get; set; }
        int Score { get; set; }
        bool KeepScore { get; set; }
        DateTime DateCreated { get; set; }
        DateTime DateUpdated { get; set; }
        DateTime DateCompleted { get; set; }
        TimeSpan TimeToSolve { get; }
        IUser User { get; set; }
        ISudokuMatrix SudokuMatrix { get; set; }
        ISudokuSolution SudokuSolution { get; set; }
        bool IsSolved();
    }
}

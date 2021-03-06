﻿using SudokuCollective.Core.Interfaces.APIModels.ResultModels;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Data.Models.ResultModels
{
    public class SolutionResult : ISolutionResult
    {
        public bool Success { get; set; }
        public bool FromCache { get; set; }
        public string Message { get; set; }
        public ISudokuSolution Solution { get; set; }

        public SolutionResult() : base()
        {
            Success = false;
            FromCache = false;
            Message = string.Empty;
            Solution = new SudokuSolution();
        }
    }
}

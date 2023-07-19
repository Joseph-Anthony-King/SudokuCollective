using System;
using System.Collections.Generic;

namespace SudokuCollective.Core.Interfaces.Models.DomainEntities
{
    public interface ISudokuSolution : IDomainEntity
    {
        List<int> SolutionList { get; set; }
        DateTime DateCreated { get; set; }
        DateTime DateSolved { get; set; }
        List<int> FirstRow { get; }
        List<int> SecondRow { get; }
        List<int> ThirdRow { get; }
        List<int> FourthRow { get; }
        List<int> FifthRow { get; }
        List<int> SixthRow { get; }
        List<int> SeventhRow { get; }
        List<int> EighthRow { get; }
        List<int> NinthRow { get; }
        IGame Game { get; set; }
        string ToString();
    }
}

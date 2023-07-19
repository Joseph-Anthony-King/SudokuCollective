using System;

namespace SudokuCollective.Core.Interfaces.Cache
{
    public interface ICachingStrategy
    {
        DateTime Light { get; }
        DateTime Medium { get; }
        DateTime Heavy { get; }
    }
}

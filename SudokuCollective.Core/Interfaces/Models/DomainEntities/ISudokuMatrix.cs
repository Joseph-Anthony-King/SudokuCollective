using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using SudokuCollective.Core.Structs;

namespace SudokuCollective.Core.Interfaces.Models.DomainEntities
{
    public interface ISudokuMatrix : IDomainEntity
    {
        IGame Game { get; set; }
        int DifficultyId { get; set; }
        IDifficulty Difficulty { get; set; }
        ICollection<ISudokuCell> SudokuCells { get; set; }
        Stopwatch Stopwatch { get; }
        List<ISudokuCell> FirstColumn { get; }
        List<ISudokuCell> SecondColumn { get; }
        List<ISudokuCell> ThirdColumn { get; }
        List<ISudokuCell> FourthColumn { get; }
        List<ISudokuCell> FifthColumn { get; }
        List<ISudokuCell> SixthColumn { get; }
        List<ISudokuCell> SeventhColumn { get; }
        List<ISudokuCell> EighthColumn { get; }
        List<ISudokuCell> NinthColumn { get; }
        List<List<ISudokuCell>> Columns { get; }
        List<ISudokuCell> FirstRegion { get; }
        List<ISudokuCell> SecondRegion { get; }
        List<ISudokuCell> ThirdRegion { get; }
        List<ISudokuCell> FourthRegion { get; }
        List<ISudokuCell> FifthRegion { get; }
        List<ISudokuCell> SixthRegion { get; }
        List<ISudokuCell> SeventhRegion { get; }
        List<ISudokuCell> EighthRegion { get; }
        List<ISudokuCell> NinthRegion { get; }
        List<List<ISudokuCell>> Regions { get; }
        List<ISudokuCell> FirstRow { get; }
        List<ISudokuCell> SecondRow { get; }
        List<ISudokuCell> ThirdRow { get; }
        List<ISudokuCell> FourthRow { get; }
        List<ISudokuCell> FifthRow { get; }
        List<ISudokuCell> SixthRow { get; }
        List<ISudokuCell> SeventhRow { get; }
        List<ISudokuCell> EighthRow { get; }
        List<ISudokuCell> NinthRow { get; }
        List<List<ISudokuCell>> Rows { get; }
        List<int> FirstColumnValues { get; }
        List<int> SecondColumnValues { get; }
        List<int> ThirdColumnValues { get; }
        List<int> FourthColumnValues { get; }
        List<int> FifthColumnValues { get; }
        List<int> SixthColumnValues { get; }
        List<int> SeventhColumnValues { get; }
        List<int> EighthColumnValues { get; }
        List<int> NinthColumnValues { get; }
        List<int> FirstRegionValues { get; }
        List<int> SecondRegionValues { get; }
        List<int> ThirdRegionValues { get; }
        List<int> FourthRegionValues { get; }
        List<int> FifthRegionValues { get; }
        List<int> SixthRegionValues { get; }
        List<int> SeventhRegionValues { get; }
        List<int> EighthRegionValues { get; }
        List<int> NinthRegionValues { get; }
        List<int> FirstRowValues { get; }
        List<int> SecondRowValues { get; }
        List<int> ThirdRowValues { get; }
        List<int> FourthRowValues { get; }
        List<int> FifthRowValues { get; }
        List<int> SixthRowValues { get; }
        List<int> SeventhRowValues { get; }
        List<int> EighthRowValues { get; }
        List<int> NinthRowValues { get; }
        bool IsValid();
        bool IsSolved();
        void SetDifficulty(IDifficulty difficulty = null);
        void GenerateSolution();
        Task Solve();
        List<int> ToIntList();
        List<int> ToDisplayedIntList();
        void HandleSudokuCellEvent(object sender, SudokuCellEventArgs e);
    }
}

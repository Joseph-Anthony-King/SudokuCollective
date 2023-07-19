using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Models;

[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Core.Utilities
{
    internal static class SudokuMatrixUtilities
    {
        /* The most straight forward method for solving a sudoku puzzle is to review the associated
         * row, column and region for any given sudoku cell and if all three lists contain only one
         * similar value then that value has to be the value for the associated sudoku cell.  This 
         * method automates that process.  The algorithm is as follows:
         * 
         *  1. Obtain the missing values for each row, column and region.
         *  2. Cycle through each sudoku cell by its index value, the the sudoku cell value is unknown
         *     we then review the associated row, columnm and region to see if there is only one value
         *     shared between all three lists.  If so that value is applied to the index sudoku cell.
         *
         * That's pretty much it.  This algorithm works for simple sudoku puzzles but if after this 
         * process the remaining sudoku cells have more than one possible value you then need a process
         * to try and eliminate these remaining values. The Sudoku Matrix solve method will do this. */
        internal static List<int> SolveByElimination(ISudokuMatrix sudokuMatrix, List<int> paramList)
        {
            var MissingFirstColumn = MissingSudokuValues(sudokuMatrix.FirstColumnValues);
            var MissingSecondColumn = MissingSudokuValues(sudokuMatrix.SecondColumnValues);
            var MissingThirdColumn = MissingSudokuValues(sudokuMatrix.ThirdColumnValues);
            var MissingFourthColumn = MissingSudokuValues(sudokuMatrix.FourthColumnValues);
            var MissingFifthColumn = MissingSudokuValues(sudokuMatrix.FifthColumnValues);
            var MissingSixthColumn = MissingSudokuValues(sudokuMatrix.SixthColumnValues);
            var MissingSeventhColumn = MissingSudokuValues(sudokuMatrix.SeventhColumnValues);
            var MissingEighthColumn = MissingSudokuValues(sudokuMatrix.EighthColumnValues);
            var MissingNinthColumn = MissingSudokuValues(sudokuMatrix.NinthColumnValues);
            var MissingFirstRegion = MissingSudokuValues(sudokuMatrix.FirstRegionValues);
            var MissingSecondRegion = MissingSudokuValues(sudokuMatrix.SecondRegionValues);
            var MissingThirdRegion = MissingSudokuValues(sudokuMatrix.ThirdRegionValues);
            var MissingFourthRegion = MissingSudokuValues(sudokuMatrix.FourthRegionValues);
            var MissingFifthRegion = MissingSudokuValues(sudokuMatrix.FifthRegionValues);
            var MissingSixthRegion = MissingSudokuValues(sudokuMatrix.SixthRegionValues);
            var MissingSeventhRegion = MissingSudokuValues(sudokuMatrix.SeventhRegionValues);
            var MissingEighthRegion = MissingSudokuValues(sudokuMatrix.EighthRegionValues);
            var MissingNinthRegion = MissingSudokuValues(sudokuMatrix.NinthRegionValues);
            var MissingFirstRow = MissingSudokuValues(sudokuMatrix.FirstRowValues);
            var MissingSecondRow = MissingSudokuValues(sudokuMatrix.SecondRowValues);
            var MissingThirdRow = MissingSudokuValues(sudokuMatrix.ThirdRowValues);
            var MissingFourthRow = MissingSudokuValues(sudokuMatrix.FourthRowValues);
            var MissingFifthRow = MissingSudokuValues(sudokuMatrix.FifthRowValues);
            var MissingSixthRow = MissingSudokuValues(sudokuMatrix.SixthRowValues);
            var MissingSeventhRow = MissingSudokuValues(sudokuMatrix.SeventhRowValues);
            var MissingEighthRow = MissingSudokuValues(sudokuMatrix.EighthRowValues);
            var MissingNinthRow = MissingSudokuValues(sudokuMatrix.NinthRowValues);

            var tmp = new SudokuMatrix(paramList);
            var count = 0;

            do
            {
                for (var i = 0; i < tmp.SudokuCells.Count; i++)
                {
                    if (i == 0)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFirstColumn,
                            ref MissingFirstRegion, ref MissingFirstRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 1)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSecondColumn,
                            ref MissingFirstRegion, ref MissingFirstRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 2)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingThirdColumn,
                            ref MissingFirstRegion, ref MissingFirstRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 3)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFourthColumn,
                            ref MissingSecondRegion, ref MissingFirstRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 4)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFifthColumn,
                            ref MissingSecondRegion, ref MissingFirstRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 5)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSixthColumn,
                            ref MissingSecondRegion, ref MissingFirstRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 6)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSeventhColumn,
                            ref MissingThirdRegion, ref MissingFirstRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 7)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingEighthColumn,
                            ref MissingThirdRegion, ref MissingFirstRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 8)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingNinthColumn,
                            ref MissingThirdRegion, ref MissingFirstRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 9)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFirstColumn,
                            ref MissingFirstRegion, ref MissingSecondRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 10)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSecondColumn,
                            ref MissingFirstRegion, ref MissingSecondRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 11)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingThirdColumn,
                            ref MissingFirstRegion, ref MissingSecondRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 12)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFourthColumn,
                            ref MissingSecondRegion, ref MissingSecondRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 13)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFifthColumn,
                            ref MissingSecondRegion, ref MissingSecondRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 14)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSixthColumn,
                            ref MissingSecondRegion, ref MissingSecondRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 15)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSeventhColumn,
                            ref MissingThirdRegion, ref MissingSecondRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 16)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingEighthColumn,
                            ref MissingThirdRegion, ref MissingSecondRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 17)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingNinthColumn,
                            ref MissingThirdRegion, ref MissingSecondRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 18)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFirstColumn,
                            ref MissingFirstRegion, ref MissingThirdRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 19)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSecondColumn,
                            ref MissingFirstRegion, ref MissingThirdRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 20)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingThirdColumn,
                            ref MissingFirstRegion, ref MissingThirdRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 21)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFourthColumn,
                            ref MissingSecondRegion, ref MissingThirdRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 22)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFifthColumn,
                            ref MissingSecondRegion, ref MissingThirdRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 23)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSixthColumn,
                            ref MissingSecondRegion, ref MissingThirdRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 24)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSeventhColumn,
                            ref MissingThirdRegion, ref MissingThirdRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 25)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingEighthColumn,
                            ref MissingThirdRegion, ref MissingThirdRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 26)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingNinthColumn,
                            ref MissingThirdRegion, ref MissingThirdRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 27)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFirstColumn,
                            ref MissingFourthRegion, ref MissingFourthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 28)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSecondColumn,
                            ref MissingFourthRegion, ref MissingFourthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 29)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingThirdColumn,
                            ref MissingFourthRegion, ref MissingFourthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 30)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFourthColumn,
                            ref MissingFifthRegion, ref MissingFourthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 31)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFifthColumn,
                            ref MissingFifthRegion, ref MissingFourthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 32)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSixthColumn,
                            ref MissingFifthRegion, ref MissingFourthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 33)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSeventhColumn,
                            ref MissingSixthRegion, ref MissingFourthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 34)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingEighthColumn,
                            ref MissingSixthRegion, ref MissingFourthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 35)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingNinthColumn,
                            ref MissingSixthRegion, ref MissingFourthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 36)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFirstColumn,
                            ref MissingFourthRegion, ref MissingFifthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 37)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSecondColumn,
                            ref MissingFourthRegion, ref MissingFifthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 38)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingThirdColumn,
                            ref MissingFourthRegion, ref MissingFifthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 39)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFourthColumn,
                            ref MissingFifthRegion, ref MissingFifthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 40)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFifthColumn,
                            ref MissingFifthRegion, ref MissingFifthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 41)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSixthColumn,
                            ref MissingFifthRegion, ref MissingFifthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 42)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSeventhColumn,
                            ref MissingSixthRegion, ref MissingFifthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 43)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingEighthColumn,
                            ref MissingSixthRegion, ref MissingFifthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 44)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingNinthColumn,
                            ref MissingSixthRegion, ref MissingFifthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 45)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFirstColumn,
                            ref MissingFourthRegion, ref MissingSixthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 46)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSecondColumn,
                            ref MissingFourthRegion, ref MissingSixthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 47)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingThirdColumn,
                            ref MissingFourthRegion, ref MissingSixthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 48)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFourthColumn,
                            ref MissingFifthRegion, ref MissingSixthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 49)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFifthColumn,
                            ref MissingFifthRegion, ref MissingSixthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 50)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSixthColumn,
                            ref MissingFifthRegion, ref MissingSixthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 51)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSeventhColumn,
                            ref MissingSixthRegion, ref MissingSixthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 52)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingEighthColumn,
                            ref MissingSixthRegion, ref MissingSixthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 53)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingNinthColumn,
                            ref MissingSixthRegion, ref MissingSixthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 54)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFirstColumn,
                            ref MissingSeventhRegion, ref MissingSeventhRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 55)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSecondColumn,
                            ref MissingSeventhRegion, ref MissingSeventhRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 56)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingThirdColumn,
                            ref MissingSeventhRegion, ref MissingSeventhRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 57)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFourthColumn,
                            ref MissingEighthRegion, ref MissingSeventhRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 58)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFifthColumn,
                            ref MissingEighthRegion, ref MissingSeventhRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 59)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSixthColumn,
                            ref MissingEighthRegion, ref MissingSeventhRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 60)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSeventhColumn,
                            ref MissingNinthRegion, ref MissingSeventhRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 61)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingEighthColumn,
                            ref MissingNinthRegion, ref MissingSeventhRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 62)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingNinthColumn,
                            ref MissingNinthRegion, ref MissingSeventhRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 63)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFirstColumn,
                            ref MissingSeventhRegion, ref MissingEighthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 64)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSecondColumn,
                            ref MissingSeventhRegion, ref MissingEighthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 65)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingThirdColumn,
                            ref MissingSeventhRegion, ref MissingEighthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 66)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFourthColumn,
                            ref MissingEighthRegion, ref MissingEighthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 67)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFifthColumn,
                            ref MissingEighthRegion, ref MissingEighthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 68)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSixthColumn,
                            ref MissingEighthRegion, ref MissingEighthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 69)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSeventhColumn,
                            ref MissingNinthRegion, ref MissingEighthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 70)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingEighthColumn,
                            ref MissingNinthRegion, ref MissingEighthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 71)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingNinthColumn,
                            ref MissingNinthRegion, ref MissingEighthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 72)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFirstColumn,
                            ref MissingSeventhRegion, ref MissingNinthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 73)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSecondColumn,
                            ref MissingSeventhRegion, ref MissingNinthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 74)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingThirdColumn,
                            ref MissingSeventhRegion, ref MissingNinthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 75)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFourthColumn,
                            ref MissingEighthRegion, ref MissingNinthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 76)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingFifthColumn,
                            ref MissingEighthRegion, ref MissingNinthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 77)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSixthColumn,
                            ref MissingEighthRegion, ref MissingNinthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 78)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingSeventhColumn,
                            ref MissingNinthRegion, ref MissingNinthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else if (i == 79)
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingEighthColumn,
                            ref MissingNinthRegion, ref MissingNinthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                    else
                    {
                        ReviewSudokuCharacterForPossibleUpdate(ref MissingNinthColumn,
                            ref MissingNinthRegion, ref MissingNinthRow, ((List<SudokuCell>)tmp.SudokuCells)[i]);
                    }
                }

                count++;

            } while (count < 10);

            var result = new SudokuMatrix(tmp.ToIntList());
            return result.ToIntList();
        }

        // This method ascertains which values a given row, column, or region is missing.
        private static List<int> MissingSudokuValues(List<int> values)
        {
            var result = new List<int>();
            var criteria = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            foreach (var i in criteria)
            {
                var AddToResult = true;

                if (values.Contains(i))
                {
                    AddToResult = false;
                }

                if (AddToResult)
                {
                    result.Add(i);
                }
            }

            return result;
        }

        /* Each sudoku cell has an associated row, column and region, this method submits the
         * relevant lists for processing and if one value is obtained it is removed from the 
         * relevant lists. */
        private static void ReviewSudokuCharacterForPossibleUpdate(ref List<int> _firstList,
            ref List<int> _secondList, ref List<int> _thirdList, ISudokuCell cell)
        {
            if (cell.Value == 0)
            {
                var i = FindSudokuValue(ref _firstList, ref _secondList, ref _thirdList, cell.Value);

                if (i != 0)
                {
                    cell.Value = i;
                }
            }
            else if (cell.Value != 0)
            {
                if (_firstList.Contains(cell.Value))
                {
                    _firstList.Remove(cell.Value);

                }
                else if (_secondList.Contains(cell.Value))
                {
                    _secondList.Remove(cell.Value);

                }
                else if (_thirdList.Contains(cell.Value))
                {
                    _thirdList.Remove(cell.Value);
                }
            }
        }

        // This method actually evalutes the row, column and region and if one value is shared it is 
        // returned.
        private static int FindSudokuValue(ref List<int> firstList, ref List<int> secondList,
            ref List<int> thirdList, int result)
        {

            if (result == 0)
            {
                var criteria = new List<int>();

                if (firstList.Count > 0 && secondList.Count > 0 && thirdList.Count > 0)
                {
                    foreach (var i in firstList)
                    {
                        var AddToCriteria = false;

                        if (secondList.Contains(i) && thirdList.Contains(i))
                        {
                            AddToCriteria = true;
                        }

                        if (AddToCriteria)
                        {
                            criteria.Add(i);
                        }
                    }

                    if (criteria.Count == 1)
                    {
                        result = criteria[0];

                        firstList.Remove(result);
                        secondList.Remove(result);
                        thirdList.Remove(result);
                    }
                }
            }

            return result;
        }
    }
}

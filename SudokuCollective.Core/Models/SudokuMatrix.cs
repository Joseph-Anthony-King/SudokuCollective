using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Extensions;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Messages;
using SudokuCollective.Core.Structs;
using SudokuCollective.Core.Utilities;
using SudokuCollective.Core.Validation.Attributes;

namespace SudokuCollective.Core.Models
{
    public class SudokuMatrix : ISudokuMatrix
    {
        #region Fields
        private List<SudokuCell> _sudokuCells = new();
        private readonly SudokuCellsValidatedAttribute _sudokuCellsValidator = new();
        private readonly Stopwatch _stopwatch = new();
        #endregion

        #region Properties
        [Required, JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonIgnore]
        IGame ISudokuMatrix.Game
        {
            get => Game;
            set => Game = (Game)value;
        }
        [JsonIgnore]
        public Game Game { get; set; }
        [Required, JsonPropertyName("difficultyId")]
        public int DifficultyId { get; set; }
        [JsonIgnore]
        IDifficulty ISudokuMatrix.Difficulty
        {
            get => Difficulty;
            set => Difficulty = (Difficulty)value;
        }
        [Required, JsonPropertyName("difficulty")]
        public Difficulty Difficulty { get; set; }
        [JsonIgnore]
        ICollection<ISudokuCell> ISudokuMatrix.SudokuCells
        {
            get => SudokuCells.ConvertAll(cell => (ISudokuCell)cell);
            set => SudokuCells = value.ToList().ConvertAll(cell => (SudokuCell)cell);
        }
        [Required, JsonPropertyName("sudokuCells"), SudokuCellsValidated(ErrorMessage = AttributeMessages.InvalidSudokuCells), JsonConverter(typeof(IDomainEntityListConverter<List<SudokuCell>>))]
        public virtual List<SudokuCell> SudokuCells
        {
            get => _sudokuCells;
            set => _sudokuCells = CoreUtilities.SetField(
                value, 
                _sudokuCellsValidator, 
                AttributeMessages.InvalidSudokuCells);
        }
        [JsonIgnore]
        public Stopwatch Stopwatch
        {
            get => _stopwatch;
        }

        #region SudokuCell List Properties
        [JsonIgnore]
        public List<ISudokuCell> FirstColumn
        { 
            get => SudokuCells
                .Where(column => column.Column == 1)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> SecondColumn
        {
            get => SudokuCells
                .Where(column => column.Column == 2)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell); 
        }
        [JsonIgnore]
        public List<ISudokuCell> ThirdColumn
        {
            get => SudokuCells
                .Where(column => column.Column == 3)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> FourthColumn
        {
            get => SudokuCells
                .Where(column => column.Column == 4)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> FifthColumn
        {
            get => SudokuCells
                .Where(column => column.Column == 5)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> SixthColumn
        {
            get => SudokuCells
                .Where(column => column.Column == 6)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> SeventhColumn
        {
            get => SudokuCells
                .Where(column => column.Column == 7)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> EighthColumn
        {
            get => SudokuCells
                .Where(column => column.Column == 8)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> NinthColumn
        {
            get => SudokuCells
                .Where(column => column.Column == 9)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }

        [JsonIgnore]
        public List<List<ISudokuCell>> Columns
        {
            get => new()
            {
                FirstColumn,
                SecondColumn,
                ThirdColumn,
                FourthColumn,
                FifthColumn,
                SixthColumn,
                SeventhColumn,
                EighthColumn,
                NinthColumn 
            };
        }

        [JsonIgnore]
        public List<ISudokuCell> FirstRegion
        {
            get => SudokuCells
                .Where(region => region.Region == 1)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> SecondRegion
        {
            get => SudokuCells
                .Where(region => region.Region == 2)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> ThirdRegion
        {
            get => SudokuCells
                .Where(region => region.Region == 3)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> FourthRegion
        {
            get => SudokuCells
                .Where(region => region.Region == 4)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> FifthRegion 
        {
            get => SudokuCells
                .Where(region => region.Region == 5)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> SixthRegion
        {
            get => SudokuCells
                .Where(region => region.Region == 6)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> SeventhRegion
        {
            get => SudokuCells
                .Where(region => region.Region == 7)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> EighthRegion
        {
            get => SudokuCells
                .Where(region => region.Region == 8)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> NinthRegion
        {
            get => SudokuCells
                .Where(region => region.Region == 9)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }

        [JsonIgnore]
        public List<List<ISudokuCell>> Regions
        {
            get => new()
            {
                FirstRegion,
                SecondRegion,
                ThirdRegion,
                FourthRegion,
                FifthRegion,
                SixthRegion,
                SeventhRegion,
                EighthRegion,
                NinthRegion
            };
        }
        [JsonIgnore]
        public List<ISudokuCell> FirstRow
        {
            get => SudokuCells
                .Where(row => row.Row == 1)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> SecondRow
        {
            get => SudokuCells
                .Where(row => row.Row == 2)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> ThirdRow
        {
            get => SudokuCells
                .Where(row => row.Row == 3)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> FourthRow
        {
            get => SudokuCells
                .Where(row => row.Row == 4)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> FifthRow
        {
            get => SudokuCells
                .Where(row => row.Row == 5)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> SixthRow
        {
            get => SudokuCells
                .Where(row => row.Row == 6)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> SeventhRow
        {
            get => SudokuCells
                .Where(row => row.Row == 7)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> EighthRow
        {
            get => SudokuCells
                .Where(row => row.Row == 8)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<ISudokuCell> NinthRow
        {
            get => SudokuCells
                .Where(row => row.Row == 9)
                .OrderBy(cell => cell.Index)
                .ToList()
                .ConvertAll(cell => (ISudokuCell)cell);
        }
        [JsonIgnore]
        public List<List<ISudokuCell>> Rows
        {
            get => new()
            {
                FirstRow,
                SecondRow,
                ThirdRow,
                FourthRow,
                FifthRow,
                SixthRow,
                SeventhRow,
                EighthRow,
                NinthRow
            };
        }
        #endregion

        #region Sudoku Cell Value Lists
        [JsonIgnore]
        public List<int> FirstColumnValues { get => SudokuCells.Where(column => column.Column == 1).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> SecondColumnValues { get => SudokuCells.Where(column => column.Column == 2).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> ThirdColumnValues { get => SudokuCells.Where(column => column.Column == 3).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> FourthColumnValues { get => SudokuCells.Where(column => column.Column == 4).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> FifthColumnValues { get => SudokuCells.Where(column => column.Column == 5).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> SixthColumnValues { get => SudokuCells.Where(column => column.Column == 6).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> SeventhColumnValues { get => SudokuCells.Where(column => column.Column == 7).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> EighthColumnValues { get => SudokuCells.Where(column => column.Column == 8).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> NinthColumnValues { get => SudokuCells.Where(column => column.Column == 9).Select(i => i.Value).Distinct().ToList(); }

        [JsonIgnore]
        public List<int> FirstRegionValues { get => SudokuCells.Where(region => region.Region == 1).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> SecondRegionValues { get => SudokuCells.Where(region => region.Region == 2).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> ThirdRegionValues { get => SudokuCells.Where(region => region.Region == 3).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> FourthRegionValues { get => SudokuCells.Where(region => region.Region == 4).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> FifthRegionValues { get => SudokuCells.Where(region => region.Region == 5).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> SixthRegionValues { get => SudokuCells.Where(region => region.Region == 6).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> SeventhRegionValues { get => SudokuCells.Where(region => region.Region == 7).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> EighthRegionValues { get => SudokuCells.Where(region => region.Region == 8).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> NinthRegionValues { get => SudokuCells.Where(region => region.Region == 9).Select(i => i.Value).Distinct().ToList(); }

        [JsonIgnore]
        public List<int> FirstRowValues { get => SudokuCells.Where(row => row.Row == 1).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> SecondRowValues { get => SudokuCells.Where(row => row.Row == 2).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> ThirdRowValues { get => SudokuCells.Where(row => row.Row == 3).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> FourthRowValues { get => SudokuCells.Where(row => row.Row == 4).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> FifthRowValues { get => SudokuCells.Where(row => row.Row == 5).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> SixthRowValues { get => SudokuCells.Where(row => row.Row == 6).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> SeventhRowValues { get => SudokuCells.Where(row => row.Row == 7).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> EighthRowValues { get => SudokuCells.Where(row => row.Row == 8).Select(i => i.Value).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> NinthRowValues { get => SudokuCells.Where(row => row.Row == 9).Select(i => i.Value).Distinct().ToList(); }

        [JsonIgnore]
        public List<int> FirstColumnDisplayedValues { get => SudokuCells.Where(column => column.Column == 1).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> SecondColumnDisplayedValues { get => SudokuCells.Where(column => column.Column == 2).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> ThirdColumnDisplayedValues { get => SudokuCells.Where(column => column.Column == 3).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> FourthColumnDisplayedValues { get => SudokuCells.Where(column => column.Column == 4).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> FifthColumnDisplayedValues { get => SudokuCells.Where(column => column.Column == 5).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> SixthColumnDisplayedValues { get => SudokuCells.Where(column => column.Column == 6).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> SeventhColumnDisplayedValues { get => SudokuCells.Where(column => column.Column == 7).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> EighthColumnDisplayedValues { get => SudokuCells.Where(column => column.Column == 8).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> NinthColumnDisplayedValues { get => SudokuCells.Where(column => column.Column == 9).Select(i => i.DisplayedValue).Distinct().ToList(); }

        [JsonIgnore]
        public List<int> FirstRegionDisplayedValues { get => SudokuCells.Where(region => region.Region == 1).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> SecondRegionDisplayedValues { get => SudokuCells.Where(region => region.Region == 2).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> ThirdRegionDisplayedValues { get => SudokuCells.Where(region => region.Region == 3).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> FourthRegionDisplayedValues { get => SudokuCells.Where(region => region.Region == 4).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> FifthRegionDisplayedValues { get => SudokuCells.Where(region => region.Region == 5).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> SixthRegionDisplayedValues { get => SudokuCells.Where(region => region.Region == 6).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> SeventhRegionDisplayedValues { get => SudokuCells.Where(region => region.Region == 7).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> EighthRegionDisplayedValues { get => SudokuCells.Where(region => region.Region == 8).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> NinthRegionDisplayedValues { get => SudokuCells.Where(region => region.Region == 9).Select(i => i.DisplayedValue).Distinct().ToList(); }

        [JsonIgnore]
        public List<int> FirstRowDisplayedValues { get => SudokuCells.Where(row => row.Row == 1).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> SecondRowDisplayedValues { get => SudokuCells.Where(row => row.Row == 2).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> ThirdRowDisplayedValues { get => SudokuCells.Where(row => row.Row == 3).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> FourthRowDisplayedValues { get => SudokuCells.Where(row => row.Row == 4).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> FifthRowDisplayedValues { get => SudokuCells.Where(row => row.Row == 5).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> SixthRowDisplayedValues { get => SudokuCells.Where(row => row.Row == 6).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> SeventhRowDisplayedValues { get => SudokuCells.Where(row => row.Row == 7).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> EighthRowDisplayedValues { get => SudokuCells.Where(row => row.Row == 8).Select(i => i.DisplayedValue).Distinct().ToList(); }
        [JsonIgnore]
        public List<int> NinthRowDisplayedValues { get => SudokuCells.Where(row => row.Row == 9).Select(i => i.DisplayedValue).Distinct().ToList(); }
        #endregion
        #endregion

        #region Constructors
        public SudokuMatrix()
        {
            Id = 0;

            var rowColumnDeliminators = new List<int>() {
                9, 18, 27, 36, 45, 54, 63, 72 };
            var firstRegionDeliminators = new List<int>() {
                1, 2, 3, 10, 11, 12, 19, 20, 21 };
            var secondRegionDeliminators = new List<int>() {
                4, 5, 6, 13, 14, 15, 22, 23, 24 };
            var thirdRegionDeliminators = new List<int>() {
                7, 8, 9, 16, 17, 18, 25, 26, 27 };
            var fourthRegionDeliminators = new List<int>() {
                28, 29, 30, 37, 38, 39, 46, 47, 48 };
            var fifthRegionDeliminators = new List<int>() {
                31, 32, 33, 40, 41, 42, 49, 50, 51 };
            var sixthRegionDeliminators = new List<int>() {
                34, 35, 36, 43, 44, 45, 52, 53, 54 };
            var seventhRegionDeliminators = new List<int>() {
                55, 56, 57, 64, 65, 66, 73, 74, 75 };
            var eighthRegionDeliminators = new List<int>() {
                58, 59, 60, 67, 68, 69, 76, 77, 78 };
            var ninthRegionDeliminators = new List<int>() {
                61, 62, 63, 70, 71, 72, 79, 80, 81 };

            var columnIndexer = 1;
            var regionIndexer = 1;
            var rowIndexer = 1;

            for (var i = 1; i < 82; i++)
            {

                if (firstRegionDeliminators.Contains(i))
                {
                    regionIndexer = 1;
                }
                else if (secondRegionDeliminators.Contains(i))
                {
                    regionIndexer = 2;
                }
                else if (thirdRegionDeliminators.Contains(i))
                {
                    regionIndexer = 3;
                }
                else if (fourthRegionDeliminators.Contains(i))
                {
                    regionIndexer = 4;
                }
                else if (fifthRegionDeliminators.Contains(i))
                {
                    regionIndexer = 5;
                }
                else if (sixthRegionDeliminators.Contains(i))
                {
                    regionIndexer = 6;
                }
                else if (seventhRegionDeliminators.Contains(i))
                {
                    regionIndexer = 7;
                }
                else if (eighthRegionDeliminators.Contains(i))
                {
                    regionIndexer = 8;
                }
                else
                {
                    regionIndexer = 9;
                }

                SudokuCells.Add(
                    new SudokuCell
                    {
                        Index = i,
                        Column = columnIndexer,
                        Region = regionIndexer,
                        Row = rowIndexer,
                        SudokuMatrixId = Id,
                        SudokuMatrix = this
                    }
                ); ;

                SudokuCells.ToList()[i - 1].SudokuCellEvent += HandleSudokuCellEvent;

                columnIndexer++;

                if (rowColumnDeliminators.Contains(i))
                {
                    columnIndexer = 1;
                    rowIndexer++;
                }
            }

            Difficulty = new Difficulty();
        }

        public SudokuMatrix(List<int> intList) : this()
        {
            for (var i = 0; i < SudokuCells.Count; i++)
            {
                SudokuCells.ToList()[i].Value = intList[i];
            }
        }

        public SudokuMatrix(
            IDifficulty difficulty,
            List<int> intList) : this(intList)
        {
            Difficulty = (Difficulty)difficulty;
            SetDifficulty(Difficulty);
        }

        public SudokuMatrix(string values) : this()
        {
            var intList = new List<int>();

            foreach (var value in values)
            {
                var s = char.ToString(value);

                if (Int32.TryParse(s, out var number))
                {
                    intList.Add(number);

                }
                else
                {
                    intList.Add(0);
                }
            }

            for (var i = 0; i < SudokuCells.Count; i++)
            {
                SudokuCells.ToList()[i].Value = intList[i];
            }
        }

        [JsonConstructor]
        public SudokuMatrix(int id, int difficultyId)
        {
            Id = id;
            DifficultyId = difficultyId;
        }
        #endregion

        #region Methods
        public bool IsValid()
        {
            if (FirstColumnValues.Count == 9 && SecondColumnValues.Count == 9
                && ThirdColumnValues.Count == 9 && FourthColumnValues.Count == 9
                && FifthColumnValues.Count == 9 && SixthColumnValues.Count == 9
                && SeventhColumnValues.Count == 9 && EighthColumnValues.Count == 9
                && NinthColumnValues.Count == 9 && FirstRegionValues.Count == 9
                && SecondRegionValues.Count == 9 && ThirdRegionValues.Count == 9
                && FourthRegionValues.Count == 9 && FifthRegionValues.Count == 9
                && SixthRegionValues.Count == 9 && SeventhRegionValues.Count == 9
                && EighthRegionValues.Count == 9 && NinthRegionValues.Count == 9
                && FirstRowValues.Count == 9 && SecondRowValues.Count == 9
                && ThirdRowValues.Count == 9 && FourthRowValues.Count == 9
                && FifthRowValues.Count == 9 && SixthRowValues.Count == 9
                && SeventhRowValues.Count == 9 && EighthRowValues.Count == 9
                && NinthRowValues.Count == 9)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual bool IsSolved()
        {
            if (FirstColumnDisplayedValues.Count == 9 && SecondColumnDisplayedValues.Count == 9
                && ThirdColumnDisplayedValues.Count == 9 && FourthColumnDisplayedValues.Count == 9
                && FifthColumnDisplayedValues.Count == 9 && SixthColumnDisplayedValues.Count == 9
                && SeventhColumnDisplayedValues.Count == 9 && EighthColumnDisplayedValues.Count == 9
                && NinthColumnDisplayedValues.Count == 9 && FirstRegionDisplayedValues.Count == 9
                && SecondRegionDisplayedValues.Count == 9 && ThirdRegionDisplayedValues.Count == 9
                && FourthRegionDisplayedValues.Count == 9 && FifthRegionDisplayedValues.Count == 9
                && SixthRegionDisplayedValues.Count == 9 && SeventhRegionDisplayedValues.Count == 9
                && EighthRegionDisplayedValues.Count == 9 && NinthRegionDisplayedValues.Count == 9
                && FirstRowDisplayedValues.Count == 9 && SecondRowDisplayedValues.Count == 9
                && ThirdRowDisplayedValues.Count == 9 && FourthRowDisplayedValues.Count == 9
                && FifthRowDisplayedValues.Count == 9 && SixthRowDisplayedValues.Count == 9
                && SeventhRowDisplayedValues.Count == 9 && EighthRowDisplayedValues.Count == 9
                && NinthRowDisplayedValues.Count == 9)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetDifficulty(IDifficulty difficulty = null)
        {
            if (difficulty != null)
            {
                Difficulty = (Difficulty)difficulty;
            }
            else
            {
                Difficulty = new Difficulty { DifficultyLevel = DifficultyLevel.MEDIUM };
            }

            foreach (var SudokuCell in SudokuCells)
            {
                SudokuCell.Hidden = true;
            }

            var patterns = new List<List<int>>();

            var pattern = new List<int>();

            if (Difficulty.DifficultyLevel == DifficultyLevel.EASY)
            {
                patterns.Add(new List<int> { 1, 2, 5, 8, 11, 12, 16, 18, 20, 22, 25, 27, 28, 30, 32, 34, 35, 40, 42, 47, 48, 50, 52, 54, 55, 57, 60, 62, 64, 66, 70, 71, 74, 77, 80, 81 });
                patterns.Add(new List<int> { 1, 2, 5, 6, 7, 8, 11, 14, 15, 17, 19, 25, 29, 32, 37, 38, 41, 44, 45, 50, 53, 57, 63, 65, 67, 68, 71, 74, 75, 76, 77, 80, 81 });
                patterns.Add(new List<int> { 1, 3, 5, 6, 7, 15, 18, 19, 21, 24, 25, 31, 34, 37, 38, 39, 40, 42, 43, 44, 45, 48, 51, 57, 58, 61, 63, 64, 67, 75, 76, 77, 79, 81 });
                patterns.Add(new List<int> { 1, 3, 5, 9, 10, 11, 15, 17, 22, 24, 25, 28, 29, 32, 37, 38, 41, 44, 45, 50, 53, 54, 57, 58, 60, 65, 67, 71, 72, 73, 77, 79, 81 });
                patterns.Add(new List<int> { 1, 3, 7, 10, 11, 12, 14, 17, 18, 20, 22, 24, 26, 28, 29, 33, 35, 39, 43, 47, 49, 53, 54, 56, 58, 60, 62, 64, 65, 68, 70, 71, 72, 75, 79, 81 });
                patterns.Add(new List<int> { 1, 4, 5, 6, 11, 14, 15, 16, 20, 22, 25, 26, 29, 32, 34, 37, 39, 43, 45, 48, 50, 53, 56, 57, 60, 62, 66, 67, 68, 71, 76, 77, 78, 81 });
                patterns.Add(new List<int> { 1, 4, 6, 9, 11, 12, 16, 19, 20, 21, 23, 26, 27, 28, 29, 33, 36, 39, 43, 46, 49, 53, 54, 55, 56, 59, 61, 62, 63, 66, 70, 71, 73, 76, 78, 81 });
                patterns.Add(new List<int> { 1, 5, 6, 8, 11, 13, 17, 20, 21, 22, 24, 25, 27, 31, 32, 33, 36, 46, 49, 50, 51, 55, 57, 58, 60, 61, 62, 65, 69, 71, 74, 76, 77, 81 });
                patterns.Add(new List<int> { 1, 10, 11, 12, 14, 16, 17, 19, 22, 24, 25, 26, 30, 33, 35, 40, 41, 42, 47, 49, 52, 56, 57, 58, 60, 63, 65, 66, 55, 70, 71, 72, 81 });
                patterns.Add(new List<int> { 2, 3, 5, 6, 7, 10, 15, 16, 18, 19, 22, 23, 25, 34, 37, 38, 39, 40, 42, 43, 44, 45, 48, 57, 59, 60, 63, 64, 66, 67, 72, 75, 76, 77, 79, 80 });
                patterns.Add(new List<int> { 2, 3, 9, 11, 13, 14, 16, 17, 18, 21, 26, 32, 33, 35, 36, 37, 40, 42, 45, 46, 47, 49, 50, 56, 61, 64, 65, 66, 68, 69, 71, 73, 79, 80 });
                patterns.Add(new List<int> { 2, 3, 10, 11, 15, 16, 17, 19, 21, 23, 24, 26, 30, 31, 32, 38, 39, 40, 42, 43, 44, 50, 51, 52, 56, 58, 59, 61, 63, 65, 66, 67, 71, 72, 79, 80 });
                patterns.Add(new List<int> { 2, 4, 6, 14, 16, 17, 19, 22, 23, 25, 26, 29, 30, 31, 36, 37, 38, 40, 42, 44, 45, 46, 51, 52, 53, 56, 57, 59, 60, 63, 65, 66, 68, 76, 78, 80 });
                patterns.Add(new List<int> { 2, 6, 7, 8, 9, 10, 12, 13, 14, 17, 19, 23, 28, 31, 32, 34, 37, 45, 48, 50, 51, 54, 59, 63, 65, 68, 69, 70, 72, 73, 74, 75, 76, 80 });
                patterns.Add(new List<int> { 3, 4, 10, 11, 13, 14, 16, 18, 22, 25, 30, 31, 33, 35, 36, 38, 41, 44, 46, 47, 49, 51, 52, 57, 60, 64, 66, 68, 69, 71, 72, 78, 79 });
                patterns.Add(new List<int> { 3, 5, 9, 10, 11, 13, 18, 20, 24, 25, 26, 29, 30, 31, 32, 34, 35, 47, 48, 50, 51, 52, 53, 56, 57, 58, 62, 64, 69, 71, 72, 73, 77, 79 });
                patterns.Add(new List<int> { 3, 5, 11, 12, 13, 17, 18, 19, 20, 22, 23, 27, 29, 30, 31, 37, 38, 39, 43, 44, 45, 51, 52, 53, 55, 59, 60, 62, 63, 64, 65, 69, 70, 71, 77, 79 });
                patterns.Add(new List<int> { 4, 6, 11, 12, 17, 18, 20, 21, 23, 24, 26, 28, 32, 33, 38, 39, 43, 44, 49, 50, 54, 56, 58, 59, 61, 62, 64, 65, 70, 71, 76, 78 });
                patterns.Add(new List<int> { 5, 6, 9, 10, 11, 14, 16, 17, 20, 24, 25, 26, 28, 29, 31, 39, 40, 42, 43, 51, 53, 54, 56, 57, 58, 62, 65, 66, 68, 71, 72, 73, 76, 77 });
                patterns.Add(new List<int> { 9, 11, 12, 14, 16, 18, 19, 21, 24, 26, 29, 32, 33, 34, 35, 37, 38, 44, 45, 47, 48, 49, 50, 53, 56, 58, 61, 63, 64, 66, 68, 70, 71, 73 });

                var indexes = new List<int>();

                for (var i = 0; i < patterns.Count; i++)
                {
                    indexes.Add(i);
                }

                Random random = new Random();

                CoreExtensions.Shuffle(indexes, random);

                pattern = patterns[indexes.FirstOrDefault()];
            }
            else if (Difficulty.DifficultyLevel == DifficultyLevel.MEDIUM)
            {
                patterns.Add(new List<int> { 1, 2, 3, 4, 8, 13, 16, 19, 22, 24, 26, 29, 34, 36, 39, 43, 46, 48, 53, 56, 58, 60, 63, 66, 69, 74, 78, 79, 80, 81 });
                patterns.Add(new List<int> { 1, 2, 4, 5, 6, 10, 11, 18, 21, 22, 27, 28, 33, 37, 38, 41, 44, 45, 49, 54, 55, 60, 61, 64, 71, 72, 76, 77, 78, 80, 81 });
                patterns.Add(new List<int> { 1, 3, 4, 8, 12, 13, 16, 17, 20, 23, 25, 27, 34, 36, 38, 44, 46, 48, 55, 57, 59, 62, 65, 66, 69, 70, 74, 78, 79, 81 });
                patterns.Add(new List<int> { 1, 2, 4, 11, 14, 17, 18, 19, 24, 26, 29, 32, 33, 36, 39, 43, 46, 49, 50, 53, 56, 58, 63, 64, 65, 68, 71, 78, 80, 81 });
                patterns.Add(new List<int> { 1, 3, 6, 7, 11, 12, 13, 17, 19, 23, 25, 29, 32, 35, 40, 41, 42, 47, 50, 53, 57, 59, 63, 65, 69, 70, 71, 75, 76, 79, 81 });
                patterns.Add(new List<int> { 1, 4, 11, 12, 14, 19, 21, 22, 24, 28, 31, 35, 39, 40, 41, 42, 43, 47, 51, 54, 58, 60, 61, 63, 68, 70, 71, 78, 81 });
                patterns.Add(new List<int> { 1, 5, 7, 12, 19, 20, 22, 24, 26, 28, 31, 35, 38, 39, 43, 44, 47, 51, 54, 56, 58, 60, 62, 63, 70, 75, 77, 81 });
                patterns.Add(new List<int> { 1, 8, 9, 10, 12, 13, 15, 21, 22, 24, 34, 36, 39, 40, 41, 42, 43, 46, 48, 58, 60, 61, 67, 69, 70, 72, 73, 74, 81 });
                patterns.Add(new List<int> { 2, 3, 5, 11, 15, 18, 24, 26, 28, 30, 31, 33, 36, 37, 38, 41, 44, 45, 46, 49, 51, 52, 54, 56, 58, 64, 67, 71, 77, 79, 80 });
                patterns.Add(new List<int> { 2, 3, 6, 8, 10, 11, 13, 18, 21, 23, 26, 28, 32, 36, 40, 41, 42, 46, 50, 54, 56, 59, 61, 64, 69, 71, 72, 74, 76, 79, 80 });
                patterns.Add(new List<int> { 2, 6, 9, 11, 13, 14, 16, 22, 24, 25, 28, 30, 31, 38, 40, 41, 42, 44, 51, 52, 54, 57, 58, 60, 66, 68, 69, 71, 73, 76, 80 });
                patterns.Add(new List<int> { 2, 8, 11, 12, 13, 14, 15, 16, 20, 24, 25, 31, 32, 35, 36, 40, 42, 46, 47, 50, 51, 57, 58, 62, 66, 67, 68, 69, 70, 71, 74, 80 });
                patterns.Add(new List<int> { 3, 4, 7, 9, 12, 15, 17, 23, 24, 25, 28, 29, 31, 33, 34, 41, 48, 49, 51, 53, 54, 57, 58, 59, 65, 67, 70, 73, 75, 78, 79 });
                patterns.Add(new List<int> { 4, 8, 9, 10, 11, 15, 21, 24, 25, 27, 31, 34, 35, 37, 41, 45, 47, 48, 51, 55, 57, 58, 61, 67, 71, 72, 73, 74, 78 });
                patterns.Add(new List<int> { 3, 5, 6, 9, 12, 13, 16, 20, 26, 29, 30, 31, 36, 37, 41, 45, 46, 51, 52, 53, 56, 62, 66, 69, 70, 73, 76, 77, 79 });
                patterns.Add(new List<int> { 5, 7, 8, 18, 19, 21, 23, 24, 25, 26, 29, 31, 35, 36, 38, 41, 44, 46, 47, 51, 53, 56, 57, 58, 59, 61, 63, 64, 74, 75, 77 });
                patterns.Add(new List<int> { 5, 8, 11, 15, 16, 19, 21, 25, 26, 31, 32, 33, 34, 37, 41, 45, 48, 49, 50, 51, 56, 57, 61, 63, 66, 67, 71, 74, 77 });
                patterns.Add(new List<int> { 5, 8, 9, 15, 16, 17, 19, 23, 26, 29, 30, 31, 34, 36, 40, 42, 46, 48, 51, 52, 53, 56, 59, 63, 65, 66, 67, 73, 74, 77 });
                patterns.Add(new List<int> { 5, 8, 9, 10, 11, 15, 16, 18, 21, 24, 25, 34, 35, 36, 37, 40, 42, 45, 46, 47, 48, 57, 58, 61, 64, 66, 67, 71, 72, 73, 74, 77 });
                patterns.Add(new List<int> { 6, 7, 9, 10, 13, 18, 21, 25, 26, 31, 33, 34, 36, 38, 41, 44, 46, 48, 49, 51, 56, 57, 61, 64, 69, 72, 73, 75, 76 });

                var indexes = new List<int>();

                for (var i = 0; i < patterns.Count; i++)
                {
                    indexes.Add(i);
                }

                Random random = new Random();

                CoreExtensions.Shuffle(indexes, random);

                pattern = patterns[indexes.FirstOrDefault()];
            }
            else if (Difficulty.DifficultyLevel == DifficultyLevel.HARD)
            {
                patterns.Add(new List<int> { 1, 2, 4, 5, 9, 10, 13, 24, 25, 26, 28, 29, 38, 41, 44, 52, 54, 56, 57, 58, 69, 72, 73, 77, 78, 80, 81 });
                patterns.Add(new List<int> { 1, 2, 6, 9, 12, 15, 16, 23, 26, 28, 31, 32, 40, 42, 50, 51, 54, 56, 59, 66, 67, 70, 73, 76, 80, 81 });
                patterns.Add(new List<int> { 1, 3, 5, 11, 14, 17, 19, 24, 25, 31, 34, 36, 37, 38, 44, 45, 46, 48, 51, 57, 58, 63, 65, 68, 71, 77, 79, 81 });
                patterns.Add(new List<int> { 1, 3, 8, 11, 14, 16, 20, 23, 24, 25, 30, 36, 40, 41, 42, 46, 52, 57, 58, 59, 62, 66, 68, 71, 74, 79, 81 });
                patterns.Add(new List<int> { 1, 4, 8, 14, 16, 20, 23, 24, 30, 33, 34, 35, 37, 41, 45, 47, 48, 49, 52, 58, 59, 62, 66, 68, 74, 78, 81 });
                patterns.Add(new List<int> { 1, 4, 19, 11, 15, 24, 25, 27, 31, 33, 35, 36, 39, 43, 46, 47, 49, 51, 55, 57, 58, 67, 71, 72, 78, 81 });
                patterns.Add(new List<int> { 1, 5, 12, 15, 22, 26, 27, 28, 30, 33, 34, 36, 38, 40, 42, 44, 46, 48, 49, 52, 54, 55, 56, 60, 67, 70, 77, 81 });
                patterns.Add(new List<int> { 1, 7, 8, 10, 12, 14, 20, 23, 26, 31, 34, 36, 37, 38, 44, 45, 46, 48, 51, 56, 59, 62, 68, 70, 72, 75, 76, 81 });
                patterns.Add(new List<int> { 1, 8, 10, 13, 16, 17, 18, 20, 24, 28, 32, 38, 40, 41, 42, 44, 50, 54, 58, 62, 64, 65, 66, 69, 72, 74, 81 });
                patterns.Add(new List<int> { 2, 4, 6, 12, 14, 17, 22, 26, 28, 31, 32, 34, 36, 40, 42, 46, 48, 50, 51, 54, 56, 60, 65, 68, 70, 76, 78, 80 });
                patterns.Add(new List<int> { 2, 5, 6, 10, 17, 21, 23, 24, 26, 30, 31, 33, 34, 35, 47, 48, 49, 51, 52, 56, 58, 59, 61, 65, 72, 76, 77, 80 });
                patterns.Add(new List<int> { 2, 5, 9, 10, 16, 19, 21, 24, 27, 31, 34, 35, 40, 42, 47, 48, 51, 55, 58, 61, 63, 65, 72, 73, 77, 80 });
                patterns.Add(new List<int> { 2, 5, 18, 20, 22, 26, 27, 31, 32, 35, 38, 39, 40, 42, 43, 44, 47, 50, 51, 55, 56, 60, 62, 64, 77, 80 });
                patterns.Add(new List<int> { 2, 9, 14, 16, 18, 19, 20, 21, 24, 29, 30, 31, 35, 37, 45, 47, 51, 52, 53, 58, 61, 62, 63, 64, 66, 68, 73, 80 });
                patterns.Add(new List<int> { 3, 5, 6, 7, 8, 11, 16, 22, 26, 30, 33, 35, 39, 40, 42, 43, 47, 49, 52, 56, 60, 66, 71, 74, 75, 76, 77, 79 });
                patterns.Add(new List<int> { 4, 6, 7, 9, 10, 11, 18, 19, 23, 30, 33, 35, 37, 45, 47, 49, 52, 59, 63, 64, 71, 72, 73, 75, 76, 78 });
                patterns.Add(new List<int> { 5, 7, 10, 11, 12, 15, 21, 22, 25, 26, 29, 32, 36, 37, 45, 46, 50, 53, 56, 57, 61, 62, 67, 70, 71, 72, 75, 77 });
                patterns.Add(new List<int> { 5, 9, 12, 14, 16, 20, 21, 22, 26, 31, 34, 36, 39, 43, 46, 48, 51, 56, 60, 61, 62, 66, 68, 70, 73, 77 });
                patterns.Add(new List<int> { 6, 9, 13, 14, 15, 17, 19, 27, 29, 30, 31, 34, 38, 44, 48, 51, 52, 53, 55, 63, 65, 67, 68, 69, 73, 76 });
                patterns.Add(new List<int> { 7, 9, 10, 14, 17, 21, 23, 24, 27, 30, 31, 32, 33, 39, 43, 49, 50, 51, 52, 55, 58, 59, 61, 65, 68, 72, 73, 75 });

                var indexes = new List<int>();

                for (var i = 0; i < patterns.Count; i++)
                {
                    indexes.Add(i);
                }

                Random random = new Random();

                CoreExtensions.Shuffle(indexes, random);

                pattern = patterns[indexes.FirstOrDefault()];
            }
            else if (Difficulty.DifficultyLevel == DifficultyLevel.EVIL)
            {
                patterns.Add(new List<int> { 1, 2, 6, 7, 13, 18, 25, 26, 30, 33, 34, 36, 47, 49, 51, 55, 68, 72, 74, 75, 78, 80, 81 });
                patterns.Add(new List<int> { 1, 2, 7, 11, 12, 14, 22, 26, 33, 42, 43, 44, 48, 50, 52, 58, 59, 64, 65, 66, 69, 75, 78 });
                patterns.Add(new List<int> { 1, 2, 12, 14, 16, 25, 26, 28, 31, 35, 42, 43, 46, 51, 52, 56, 57, 59, 66, 68, 71, 74, 78 });
                patterns.Add(new List<int> { 1, 5, 10, 11, 12, 19, 23, 24, 30, 35, 40, 41, 43, 47, 52, 58, 62, 63, 69, 75, 78, 81 });
                patterns.Add(new List<int> { 2, 3, 9, 13, 18, 24, 25, 28, 30, 32, 40, 41, 42, 48, 50, 56, 61, 62, 64, 73, 78, 80 });
                patterns.Add(new List<int> { 2, 4, 9, 13, 17, 18, 21, 22, 24, 27, 33, 36, 37, 48, 49, 61, 63, 64, 66, 69, 75, 76, 79 });
                patterns.Add(new List<int> { 2, 5, 8, 9, 16, 23, 29, 24, 37, 39, 40, 41, 47, 51, 55, 60, 63, 66, 69, 72, 73, 77, 81 });
                patterns.Add(new List<int> { 2, 5, 9, 12, 14, 22, 26, 27, 28, 30, 34, 35, 37, 40, 50, 55, 57, 58, 61, 67, 69, 77, 80 });
                patterns.Add(new List<int> { 2, 6, 11, 16, 17, 19, 22, 32, 36, 40, 41, 42, 50, 53, 54, 55, 57, 61, 71, 75, 76, 80 });
                patterns.Add(new List<int> { 2, 7, 9, 11, 12, 15, 22, 26, 34, 36, 38, 39, 41, 45, 48, 56, 62, 66, 68, 72, 76, 77, 80 });
                patterns.Add(new List<int> { 2, 7, 9, 15, 19, 20, 22, 25, 30, 32, 35, 37, 40, 41, 47, 51, 54, 58, 63, 66, 74, 76, 81 });
                patterns.Add(new List<int> { 2, 9, 10, 13, 15, 18, 23, 30, 34, 36, 37, 42, 45, 47, 49, 53, 57, 59, 60, 62, 65, 77 });
                patterns.Add(new List<int> { 3, 9, 10, 11, 12, 21, 25, 27, 31, 35, 36, 38, 42, 46, 49, 56, 59, 61, 70, 77, 78, 80 });
                patterns.Add(new List<int> { 3, 13, 15, 18, 25, 26, 27, 29, 35, 40, 41, 46, 50, 51, 52, 62, 63, 64, 66, 67, 75, 77, 79 });
                patterns.Add(new List<int> { 4, 5, 17, 19, 20, 27, 29, 32, 36, 48, 49, 51, 54, 56, 59, 60, 62, 64, 69, 70, 75, 78, 80 });
                patterns.Add(new List<int> { 4, 6, 17, 18, 19, 21, 23, 28, 32, 33, 36, 38, 47, 50, 53, 64, 69, 70, 74, 76, 78, 81 });
                patterns.Add(new List<int> { 4, 7, 8, 12, 14, 16, 23, 28, 33, 37, 41, 42, 46, 47, 48, 56, 63, 66, 71, 76, 78, 81 });
                patterns.Add(new List<int> { 4, 8, 13, 16, 17, 20, 24, 29, 32, 33, 40, 45, 48, 50, 60, 62, 64, 66, 69, 72, 75, 81 });
                patterns.Add(new List<int> { 5, 15, 18, 22, 23, 25, 26, 27, 30, 35, 40, 43, 44, 46, 47, 51, 55, 62, 67, 71, 73, 76, 78 });
                patterns.Add(new List<int> { 7, 11, 13, 16, 19, 22, 24, 28, 32, 35, 36, 38, 40, 49, 54, 55, 59, 61, 71, 74, 76, 79 });

                var indexes = new List<int>();

                for (var i = 0; i < patterns.Count; i++)
                {
                    indexes.Add(i);
                }

                Random random = new Random();

                CoreExtensions.Shuffle(indexes, random);

                pattern = patterns[indexes.FirstOrDefault()];
            }
            else
            {
                // do nothing...
            }

            if (Difficulty.DifficultyLevel == DifficultyLevel.TEST ||
                Difficulty.DifficultyLevel == DifficultyLevel.NULL)
            {
                foreach (var sudokuCell in SudokuCells)
                {
                    sudokuCell.Hidden = false;
                }
            }
            else
            {
                foreach (var sudokuCell in SudokuCells)
                {
                    if (pattern.Contains(sudokuCell.Index))
                    {
                        sudokuCell.Hidden = false;
                    }
                    else
                    {
                        sudokuCell.Hidden = true;
                    }
                }
            }
        }

        public void GenerateSolution()
        {
            do
            {
                ZeroOutSudokuCells();

                foreach (var sudokuCell in SudokuCells)
                {
                    if (sudokuCell.Value == 0 &&
                        sudokuCell
                            .AvailableValues
                            .Where(a => a.Available == true)
                            .ToList()
                            .Count > 0)
                    {
                        var availableValues = sudokuCell
                            .AvailableValues
                            .Where(a => a.Available == true)
                            .ToList();

                        var indexList = new List<int>();

                        for (var i = 0; i < availableValues.Count; i++)
                        {
                            indexList.Add(i);
                        }

                        Random random = new Random();

                        CoreExtensions.Shuffle(indexList, random);

                        sudokuCell.Value = availableValues[indexList.FirstOrDefault()].Value;
                    }
                }

            } while (!IsValid());
        }

        public async Task Solve()
        {
            await Task.Run(() =>
            {
                _stopwatch.Reset();

                _stopwatch.Start();

                var tmp = new SudokuMatrix(this.ToIntList());

                var seed = SudokuMatrixUtilities.SolveByElimination(tmp, tmp.ToIntList());

                if (seed.Contains(0))
                {
                    SudokuMatrix matrix;

                    do
                    {
                        if (!_stopwatch.IsRunning)
                        {
                            _stopwatch.Start();
                        }

                        matrix = new SudokuMatrix(seed);

                        foreach (var sudokuCell in matrix.SudokuCells)
                        {
                            if (sudokuCell.Value == 0 &&
                                sudokuCell
                                    .AvailableValues
                                    .Where(a => a.Available == true)
                                    .ToList()
                                    .Count > 0)
                            {
                                var availableValues = sudokuCell
                                    .AvailableValues
                                    .Where(a => a.Available == true)
                                    .ToList();

                                var indexList = new List<int>();

                                for (var i = 0; i < availableValues.Count; i++)
                                {
                                    indexList.Add(i);
                                }

                                Random random = new Random();

                                CoreExtensions.Shuffle(indexList, random);

                                sudokuCell.Value = availableValues[indexList.FirstOrDefault()].Value;
                            }
                        }

                        _stopwatch.Stop();

                    } while (_stopwatch.Elapsed.TotalMinutes < 3 && !matrix.IsValid());

                    seed = matrix.ToIntList();
                }

                var result = new SudokuMatrix(seed);

                SudokuCells = result.SudokuCells;

                if (_stopwatch.IsRunning)
                {
                    _stopwatch.Stop();
                }
            });
        }

        private void ZeroOutSudokuCells()
        {
            foreach (var SudokuCell in SudokuCells)
            {
                SudokuCell.Value = 0;

                foreach (var availableValue in SudokuCell.AvailableValues)
                {
                    availableValue.Available = true;
                }
            }
        }

        public List<int> ToIntList()
        {
            var result = new List<int>();

            foreach (var SudokuCell in SudokuCells)
            {
                result.Add(SudokuCell.Value);
            }

            return result;
        }

        public List<int> ToDisplayedIntList()
        {
            var result = new List<int>();

            foreach (var SudokuCell in SudokuCells)
            {
                result.Add(SudokuCell.DisplayedValue);
            }

            return result;
        }

        public override string ToString()
        {
            var result = new StringBuilder();

            foreach (var SudokuCell in SudokuCells)
            {
                result.Append(SudokuCell);
            }

            return result.ToString();
        }

        public string ToJson() => JsonSerializer.Serialize(
            this,
            new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });

        public IDomainEntity Cast<T>() => throw new System.NotImplementedException();
        #endregion

        #region Event Handlers
        public void HandleSudokuCellEvent(
            object sender,
            SudokuCellEventArgs e)
        {
            foreach (var sudokuCell in SudokuCells)
            {
                if (sudokuCell.Column == e.Column)
                {
                    sudokuCell.UpdateAvailableValues(e.Value);
                }
                else if (sudokuCell.Region == e.Region)
                {
                    sudokuCell.UpdateAvailableValues(e.Value);
                }
                else if (sudokuCell.Row == e.Row)
                {
                    sudokuCell.UpdateAvailableValues(e.Value);
                }
                else
                {
                    // do nothing...
                }
            }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Core.Messages;
using SudokuCollective.Data.Validation.Attributes;

namespace SudokuCollective.Data.Models.Payloads
{
    public class SolutionPayload : ISolutionPayload
    {
        #region Fields
        private List<int> _firstRow = new();
        private List<int> _secondRow = new();
        private List<int> _thirdRow = new();
        private List<int> _fourthRow = new();
        private List<int> _fifthRow = new();
        private List<int> _sixthRow = new();
        private List<int> _seventhRow = new();
        private List<int> _eighthRow = new();
        private List<int> _ninthRow = new();
        private readonly RowValidatedAttribute _rowValidator = new();
        #endregion

        #region Properties
        [Required, RowValidated, JsonPropertyName("firstRow")]
        public List<int> FirstRow
        {
            get
            {
                return _firstRow;
            }
            set
            {
                if (value != null && _rowValidator.IsValid(value))
                {
                    _firstRow = value;
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidFirstRow);
                }
            }
        }
        [Required, RowValidated, JsonPropertyName("secondRow")]
        public List<int> SecondRow
        {
            get
            {
                return _secondRow;
            }
            set
            {
                if (value != null && _rowValidator.IsValid(value))
                {
                    _secondRow = value;
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidSecondRow);
                }
            }
        }
        [Required, RowValidated, JsonPropertyName("thirdRow")]
        public List<int> ThirdRow
        {
            get
            {
                return _thirdRow;
            }
            set
            {
                if (value != null && _rowValidator.IsValid(value))
                {
                    _thirdRow = value;
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidThirdRow);
                }
            }
        }
        [Required, RowValidated, JsonPropertyName("fourthRow")]
        public List<int> FourthRow
        {
            get
            {
                return _fourthRow;
            }
            set
            {
                if (value != null && _rowValidator.IsValid(value))
                {
                    _fourthRow = value;
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidFourthRow);
                }
            }
        }
        [Required, RowValidated, JsonPropertyName("fifthRow")]
        public List<int> FifthRow
        {
            get
            {
                return _fifthRow;
            }
            set
            {
                if (value != null && _rowValidator.IsValid(value))
                {
                    _fifthRow = value;
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidFifthRow);
                }
            }
        }
        [Required, RowValidated, JsonPropertyName("sixthRow")]
        public List<int> SixthRow
        {
            get
            {
                return _sixthRow;
            }
            set
            {
                if (value != null && _rowValidator.IsValid(value))
                {
                    _sixthRow = value;
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidSixthRow);
                }
            }
        }
        [Required, RowValidated, JsonPropertyName("seventhRow")]
        public List<int> SeventhRow
        {
            get
            {
                return _seventhRow;
            }
            set
            {
                if (value != null && _rowValidator.IsValid(value))
                {
                    _seventhRow = value;
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidSeventhRow);
                }
            }
        }
        [Required, RowValidated, JsonPropertyName("eighthRow")]
        public List<int> EighthRow
        {
            get
            {
                return _eighthRow;
            }
            set
            {
                if (value != null && _rowValidator.IsValid(value))
                {
                    _eighthRow = value;
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidEighthRow);
                }
            }
        }
        [Required, RowValidated, JsonPropertyName("ninthRow")]
        public List<int> NinthRow
        {
            get
            {
                return _ninthRow;
            }
            set
            {
                if (value != null && _rowValidator.IsValid(value))
                {
                    _ninthRow = value;
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidNinthRow);
                }
            }
        }
        #endregion

        #region Constructors
        public SolutionPayload() { }

        public SolutionPayload(
            int[] firstRow, 
            int[] secondRow, 
            int[] thirdRow, 
            int[] fourthRow, 
            int[] fifthRow, 
            int[] sixthRow, 
            int[] seventhRow, 
            int[] eighthRow, 
            int[] ninthRow)
        {
            FirstRow = firstRow.ToList();
            SecondRow = secondRow.ToList();
            ThirdRow = thirdRow.ToList();
            FourthRow = fourthRow.ToList();
            FifthRow = fifthRow.ToList();
            SixthRow = sixthRow.ToList();
            SeventhRow = seventhRow.ToList();
            EighthRow = eighthRow.ToList();
            NinthRow = ninthRow.ToList();
        }

        public SolutionPayload(
            List<int> firstRow, 
            List<int> secondRow, 
            List<int> thirdRow, 
            List<int> fourthRow, 
            List<int> fifthRow, 
            List<int> sixthRow, 
            List<int> seventhRow, 
            List<int> eighthRow, 
            List<int> ninthRow)
        {
            FirstRow = firstRow;
            SecondRow = secondRow;
            ThirdRow = thirdRow;
            FourthRow = fourthRow;
            FifthRow = fifthRow;
            SixthRow = sixthRow;
            SeventhRow = seventhRow;
            EighthRow = eighthRow;
            NinthRow = ninthRow;
        }
        #endregion

        #region Methods
        public static implicit operator JsonElement(SolutionPayload v)
        {
            return JsonSerializer.SerializeToElement(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
        #endregion
    }
}

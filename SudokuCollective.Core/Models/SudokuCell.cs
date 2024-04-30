using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Messages;
using SudokuCollective.Core.Structs;

namespace SudokuCollective.Core.Models
{
    public class SudokuCell : ISudokuCell
    {
        #region Fields
        private int _index = 0;
        private int _column = 0;
        private int _region = 0;
        private int _row = 0;
        private int _value = 0;
        private int _displayValue = 0;
        #endregion

        #region Properties
        [Required, JsonPropertyName("id")]
        public int Id { get; set; }
        [Required, JsonPropertyName("index"), Range(1, 81, ErrorMessage = AttributeMessages.InvalidIndex)]
        public int Index
        {
            get => _index;
            set => _index = SetField(value, 1, 81, AttributeMessages.InvalidIndex);
        }
        [Required, JsonPropertyName("column"), Range(1, 9, ErrorMessage = AttributeMessages.InvalidColumn)]
        public int Column
        {
            get => _column;
            set => _column = SetField(value, 1, 9, AttributeMessages.InvalidColumn);
        }
        [Required, JsonPropertyName("region"), Range(1, 9, ErrorMessage = AttributeMessages.InvalidRegion)]
        public int Region
        {
            get => _region;
            set => _region = SetField(value, 1, 9, AttributeMessages.InvalidRegion);
        }
        [Required, JsonPropertyName("row"), Range(1, 9, ErrorMessage = AttributeMessages.InvalidRow)]
        public int Row
        {
            get => _row;
            set => _row = SetField(value, 1, 9, AttributeMessages.InvalidRow);
        }
        [Required, JsonPropertyName("value"), Range(1, 9, ErrorMessage = AttributeMessages.InvalidValue)]
        public int Value
        {
            get => _value;
            set => _value = SetValue(value);
        }
        [Required, JsonPropertyName("displayedValue"), Range(0, 9, ErrorMessage = AttributeMessages.InvalidDisplayedValue)]
        public int DisplayedValue
        {
            get => GetDisplayedValue();
            set => _displayValue = SetField(value, 0, 9, AttributeMessages.InvalidDisplayedValue);
        }
        [Required, JsonPropertyName("hidden")]
        public bool Hidden { get; set; }
        [Required, JsonPropertyName("sudokuMatrixId")]
        public int SudokuMatrixId { get; set; }
        [JsonIgnore]
        ISudokuMatrix ISudokuCell.SudokuMatrix
        {
            get => SudokuMatrix;
            set => SudokuMatrix = (SudokuMatrix)value;
        }
        [JsonIgnore]
        public virtual SudokuMatrix SudokuMatrix { get; set; }
        [JsonIgnore]
        ICollection<IAvailableValue> ISudokuCell.AvailableValues
        {
            get => AvailableValues.ConvertAll(av => (IAvailableValue)av);
            set => AvailableValues = value.ToList().ConvertAll(av => (AvailableValue)av);
        }
        [JsonIgnore]
        public List<AvailableValue> AvailableValues { get; set; }
        #endregion

        #region Constructors
        public SudokuCell()
        {
            Id = 0;
            SudokuMatrixId = 0;
            Hidden = true;
            AvailableValues = [];

            for (var i = 1; i <= 9; i++)
            {
                AvailableValues.Add(
                        new AvailableValue
                        {
                            Value = i,
                            Available = true,
                            NumberOfSelections = 0,
                        }
                    );
            }
        }

        public SudokuCell(
            int index,
            int column,
            int region,
            int row,
            int matrixID) : this()
        {
            Index = index;
            Column = column;
            Region = region;
            Row = row;
            SudokuMatrixId = matrixID;
        }

        public SudokuCell(
            int index,
            int column,
            int region,
            int row,
            int value,
            int matrixId) : this()
        {
            Index = index;
            Column = column;
            Region = region;
            Row = row;
            SudokuMatrixId = matrixId;
            Value = value;

            if (Value != 0)
            {
                foreach (var availableValue in AvailableValues)
                {
                    availableValue.Available = false;
                }
            }
        }

        [JsonConstructor]
        public SudokuCell(
            int id,
            int index,
            int column,
            int region,
            int row,
            int value,
            int displayedValue,
            bool hidden,
            int sudokuMatrixId)
        {
            Id = id;
            Index = index;
            Column = column;
            Region = region;
            Row = row;
            _value = value;
            DisplayedValue = displayedValue;
            Hidden = hidden;
            SudokuMatrixId = sudokuMatrixId;
            AvailableValues = new List<AvailableValue>();

            var availability = true;

            if (value > 0)
            {
                availability = false;
            }

            for (var i = 1; i <= 9; i++)
            {
                AvailableValues.Add(
                    new AvailableValue {
                        Value = i,
                        Available = availability
                    });
            }
        }
        #endregion

        #region Methods
        public void UpdateAvailableValues(int i)
        {
            var availableValue = AvailableValues
                .FirstOrDefault(a => a.Value == i && a.Available);

            if (availableValue != null)
            {
                availableValue.Available = false;

                var valuesStillAvailable = AvailableValues
                    .Where(a => a.Available).ToList();

                if (valuesStillAvailable.Count == 1)
                {
                    Value = valuesStillAvailable[0].Value;

                    valuesStillAvailable[0].Available = false;
                }
            }
        }

        public int ToInt32() => DisplayedValue;

        public override string ToString() => DisplayedValue.ToString();

        public string ToValuesString() => Value.ToString();

        public string ToJson() => JsonSerializer.Serialize(
            this,
            new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });

        public IDomainEntity Cast<T>() => throw new System.NotImplementedException();
        #endregion

        #region Events
        public event EventHandler<SudokuCellEventArgs> SudokuCellEvent;

        public virtual void OnSuccessfulSudokuCellUpdate(SudokuCellEventArgs e)
        {
            SudokuCellEvent.Invoke(this, e);
        }

        private static int SetField(
            int value, 
            int minValue, 
            int maxValue, 
            string errorMessage)
        {
            if (value >= minValue && value <= maxValue)
            {
                return value;
            }
            else
            {
                throw new ArgumentException(errorMessage);
            }
        }

        private int SetValue(int value)
        {
            if (value == 0)
            {
                if (Value != 0)
                {
                    foreach (var availableValue in AvailableValues)
                    {
                        availableValue.Available = true;
                    }
                }
            }
            else if (value >= 1 && value <= 9)
            {
                foreach (var availableValue in AvailableValues)
                {
                    availableValue.Available = false;
                }

                if (SudokuCellEvent != null)
                {
                    OnSuccessfulSudokuCellUpdate(
                        new SudokuCellEventArgs(
                            Index,
                            value,
                            Column,
                            Region,
                            Row
                        )
                    );
                }
            }
            else
            {
                throw new ArgumentException(AttributeMessages.InvalidValue);
            }

            return value;
        }

        private int GetDisplayedValue()
        {
            if (!Hidden)
            {
                return _value;
            }
            else
            {
                return _displayValue;
            }
        }
        #endregion
    }
}

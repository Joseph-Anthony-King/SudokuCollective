using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Core.Messages;
using SudokuCollective.Core.Models;
using SudokuCollective.Core.Validation.Attributes;

namespace SudokuCollective.Data.Models.Payloads
{
    public class GamePayload : IGamePayload
    {
        private List<SudokuCell> _sudokuCells = new();
        private readonly SudokuCellsValidatedAttribute _sudokuCellsValidator = new();

        [Required, SudokuCellsValidated(ErrorMessage = AttributeMessages.InvalidSudokuCells), JsonPropertyName("sudokuCells")]
        public List<SudokuCell> SudokuCells
        {
            get
            {
                return _sudokuCells;
            }
            set
            {
                if (value != null && _sudokuCellsValidator.IsValid(value))
                {
                    _sudokuCells = value;
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidSudokuCells);
                }
            }
        }
        
        public GamePayload()
        {
            SudokuCells = new List<SudokuCell>(); ;
        }

        public GamePayload(SudokuCell[] sudokuCells)
        {
            SudokuCells = sudokuCells.ToList(); ;
        }

        public GamePayload(List<SudokuCell> sudokuCells)
        {
            SudokuCells = sudokuCells;
        }

        public static implicit operator JsonElement(GamePayload v)
        {
            return JsonSerializer.SerializeToElement(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}

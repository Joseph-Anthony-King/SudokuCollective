using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Models
{
    public class AvailableValue : IAvailableValue
    {
        [Required, JsonPropertyName("value")]
        public int Value { get; set; }
        [Required, JsonPropertyName("available")]
        public bool Available { get; set; }
    }
}

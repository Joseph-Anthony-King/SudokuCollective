using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests;

namespace SudokuCollective.Data.Models.Requests
{
    public class AnnonymousGameRequest : IAnnonymousGameRequest
    {
        [Required, JsonPropertyName("difficultyLevel")]
        public DifficultyLevel DifficultyLevel { get; set; }

        public AnnonymousGameRequest()
        {
            DifficultyLevel = DifficultyLevel.NULL;
        }

        public AnnonymousGameRequest(int difficultyLevel)
        {
            DifficultyLevel = (DifficultyLevel)difficultyLevel;
        }

        public AnnonymousGameRequest(DifficultyLevel difficultyLevel)
        {
            DifficultyLevel = difficultyLevel;
        }
    }
}

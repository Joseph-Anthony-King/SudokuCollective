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
        [Required, JsonPropertyName("appId")]
        public int AppId { get; set; }

        public AnnonymousGameRequest()
        {
            DifficultyLevel = DifficultyLevel.NULL;
            AppId = 0;
        }

        public AnnonymousGameRequest(int difficultyLevel, int appId)
        {
            DifficultyLevel = (DifficultyLevel)difficultyLevel;
            AppId = appId;
        }

        public AnnonymousGameRequest(DifficultyLevel difficultyLevel, int appId)
        {
            DifficultyLevel = difficultyLevel;
            AppId = appId;
        }
    }
}

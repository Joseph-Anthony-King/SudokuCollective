using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;

namespace SudokuCollective.Data.Models.Results
{
    public class EmailConfirmationSentResult : IEmailConfirmationSentResult
    {
        [JsonPropertyName("emailConfirmationSent")]
        public bool EmailConfirmationSent { get; set; }

        public EmailConfirmationSentResult()
        {
            EmailConfirmationSent = false;
        }

        public EmailConfirmationSentResult(bool emailConfirmationSent)
        {
            EmailConfirmationSent = emailConfirmationSent;
        }
    }
}

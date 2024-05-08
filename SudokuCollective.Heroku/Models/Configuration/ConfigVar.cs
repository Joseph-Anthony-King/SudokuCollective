using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("SudokuCollective.Cache")]
[assembly: InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.HerokuIntegration.Models.Configuration
{
    internal class ConfigVar
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }

        internal ConfigVar()
        {
            Name = string.Empty;
            Value = string.Empty;
        }

        [JsonConstructor]
        internal ConfigVar(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}

using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("SudokuCollective.Cache")]
[assembly: InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.HerokuIntegration.Models.Requests
{
    internal class HerokuRedisConnectionStrings
    {
        [JsonPropertyName("REDIS:URL")]
        public string RedisUrl { get; set; }

        internal HerokuRedisConnectionStrings()
        {
            RedisUrl = string.Empty;
        }

        [JsonConstructor]
        internal HerokuRedisConnectionStrings(string redisUrl) : base ()
        {
            RedisUrl = redisUrl;
        }
    }
}

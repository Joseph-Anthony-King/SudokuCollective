using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("SudokuCollective.Cache")]
[assembly: InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.HerokuIntegration.Models.Requests
{
    internal class HerokuRedisConnectionStrings
    {
        [JsonPropertyName("REDIS:TLS_URL")]
        public string RedisTlsUrl { get; set; }
        [JsonPropertyName("REDIS:URL")]
        public string RedisUrl { get; set; }

        internal HerokuRedisConnectionStrings()
        {
            RedisTlsUrl = string.Empty;
            RedisUrl = string.Empty;
        }

        [JsonConstructor]
        internal HerokuRedisConnectionStrings(
            string redisTlsUrl,
            string redisUrl)
        {
            RedisTlsUrl = redisTlsUrl;
            RedisUrl = redisUrl;
        }
    }
}

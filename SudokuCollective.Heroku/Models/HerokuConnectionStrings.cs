using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Heroku.Models
{
    internal class HerokuConnectionStrings
    {
        [JsonPropertyName("REDIS:TLS_URL")]
        public string RedisTlsUrl { get; set; }
        [JsonPropertyName("REDIS:URL")]
        public string RedisUrl { get; set; }

        internal HerokuConnectionStrings() 
        {
            RedisTlsUrl = string.Empty;
            RedisUrl = string.Empty;
        }

        [JsonConstructor]
        internal HerokuConnectionStrings(
            string redisTlsUrl,
            string redisUrl)
        {
            RedisTlsUrl = redisTlsUrl;
            RedisUrl = redisUrl;
        }
    }
}

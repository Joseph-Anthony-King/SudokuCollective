using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Heroku.Models
{
    internal class HerokuConnectionStrings
    {
        [JsonPropertyName("REDIS:TLS_URL"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        internal string? RedisTlsUrl { get; set; }
        [JsonPropertyName("REDIS:URL"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        internal string? RedisUrl { get; set; }

        internal HerokuConnectionStrings() 
        {
            RedisTlsUrl = null;
            RedisUrl = null;
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

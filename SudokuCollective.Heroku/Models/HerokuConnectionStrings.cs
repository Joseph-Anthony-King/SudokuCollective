using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Heroku.Models
{
    internal class HerokuConnectionStrings
    {
        [JsonPropertyName("DATABASE_URL"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        internal string? DatabaseUrl { get; set; }
        [JsonPropertyName("REDIS:TLS_URL"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        internal string? RedisTlsUrl { get; set; }
        [JsonPropertyName("REDIS:URL"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        internal string? RedisUrl { get; set; }

        internal HerokuConnectionStrings() 
        {
            DatabaseUrl = null;
            RedisTlsUrl = null;
            RedisUrl = null;
        }

        [JsonConstructor]
        internal HerokuConnectionStrings(
            string databaseUrl,
            string redisTlsUrl,
            string redisUrl)
        {
            DatabaseUrl = databaseUrl;
            RedisTlsUrl = redisTlsUrl;
            RedisUrl = redisUrl;
        }
    }
}

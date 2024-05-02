using System.Runtime.CompilerServices;
using StackExchange.Redis;

[assembly: InternalsVisibleTo("SudokuCollective.Api")]
namespace SudokuCollective.Heroku
{
    internal static class HerokuIntegration
    {
        internal static string GetHerokuPostgresConnectionString()
        {
            // get the connection string from the ENV variables
            var connectionUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

            // parse the connection string
            var databaseUri = new Uri(connectionUrl!);

            var db = databaseUri.LocalPath.TrimStart('/');
            string[] userInfo = databaseUri.UserInfo.Split(':', StringSplitOptions.RemoveEmptyEntries);

            return $"User ID={userInfo[0]};Password={userInfo[1]};Host={databaseUri.Host};Port={databaseUri.Port};Database={db};Pooling=true;SSL Mode=Require;Trust Server Certificate=True;";
        }

        internal static ConfigurationOptions GetHerokuRedisConfigurationOptions()
        {
            // Get the connection string from the ENV variables in staging
            string redisUrlString = Environment.GetEnvironmentVariable("REDIS_URL")!;

            // parse the connection string
            var redisUri = new Uri(redisUrlString);
            var userInfo = redisUri.UserInfo.Split(':');

            var config = new ConfigurationOptions
            {
                EndPoints = { { redisUri.Host, redisUri.Port } },
                Password = userInfo[1],
                AbortOnConnectFail = true,
                ConnectRetry = 3,
                Ssl = true,
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
            };

            // Disable peer certificate verification
            config.CertificateValidation += delegate { return true; };

            return config;
        }
    }
}

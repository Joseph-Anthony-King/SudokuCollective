using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SudokuCollective.Api")]
namespace SudokuCollective.Heroku
{
    internal static class HerokuService
    {
        internal static void UpdateHerokuRedisConnectionStringAsync()
        {
            // Connect to Heroku API

            // Obtain Heroku Redis Settings

            // Transform redis connection settings

            // Update App redis connection
        }

        internal static void UpdateHerokuPostgresConnectionStringAsync()
        {
            // Connect to Heroku API

            // Obtain Heroku Postgres Settings

            // Transform db connection settings

            // Update App db connection
        }
    }
}

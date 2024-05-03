namespace SudokuCollective.Heroku.Models
{
    internal class HerokuConnectionStrings
    {
        internal string? DATABASE_URL { get; set; }
        internal string? REDIS_TLS_URL { get; set; }
        internal string? REDIS_URL { get; set; }
    }
}

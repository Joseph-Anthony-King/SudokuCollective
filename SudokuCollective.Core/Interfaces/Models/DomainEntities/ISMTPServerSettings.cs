namespace SudokuCollective.Core.Interfaces.Models.DomainEntities
{
    public interface ISMTPServerSettings : IDomainEntity
    {
        string SmtpServer { get; set; }
        int Port { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        string FromEmail { get; set; }
        int AppId { get; set; }
        IApp App { get; set; }
        bool AreSettingsValid();
        void Sanitize();
    }
}

namespace SudokuCollective.Core.Interfaces.ServiceModels
{
    public interface IEmailMetaData
    {
        string SmtpServer { get; set; }
        int Port { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        string FromEmail { get; set; }
    }
}

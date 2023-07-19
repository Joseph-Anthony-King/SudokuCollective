using SudokuCollective.Core.Interfaces.ServiceModels;

namespace SudokuCollective.Data.Models
{
    public class EmailMetaData : IEmailMetaData
    {
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FromEmail { get; set; }

        public EmailMetaData()
        {
            SmtpServer = string.Empty;
            Port = 0;
            UserName = string.Empty;
            Password = string.Empty;
            FromEmail = string.Empty;
        }

        public EmailMetaData(
            string smtpResponse, 
            int port, 
            string userName, 
            string password, 
            string fromEmail)
        {
            SmtpServer = smtpResponse;
            Port = port;
            UserName = userName;
            Password = password;
            FromEmail = fromEmail;
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Messages;
using SudokuCollective.Logs;
using SudokuCollective.Logs.Utilities;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using Request = SudokuCollective.Logs.Models.Request;

namespace SudokuCollective.Data.Services
{
    public class EmailService(
        IEmailMetaData emailMetaData,
        IRequestService requestService,
        IAppsRepository<App> appsRepository,
        ILogger<EmailService> logger) : IEmailService
    {
        private readonly IEmailMetaData _emailMetaData = emailMetaData;
        private readonly IRequestService _requestService = requestService;
        private readonly IAppsRepository<App> _appsRepository = appsRepository;
        private readonly ILogger<EmailService> _logger = logger;

        public async Task<bool> SendAsync(string to, string subject, string html, int appId)
        {
            try
            {
                ArgumentException.ThrowIfNullOrEmpty(to, nameof(to));

                ArgumentException.ThrowIfNullOrEmpty(subject, nameof(subject));

                ArgumentException.ThrowIfNullOrEmpty(html, nameof(html));

                ArgumentNullException.ThrowIfNull(appId, nameof(appId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(appId, nameof(appId));

                var app = (App)(await _appsRepository.GetAsync(appId)).Object;

                ArgumentNullException.ThrowIfNull(app, nameof(app));

                IEmailMetaData emailMetaData;

                if (app.UseCustomSMTPServer && app.SMTPServerSettings.AreSettingsValid())
                {
                    emailMetaData = new EmailMetaData(
                        app.SMTPServerSettings.SmtpServer,
                        app.SMTPServerSettings.Port,
                        app.SMTPServerSettings.UserName,
                        app.SMTPServerSettings.Password,
                        app.SMTPServerSettings.FromEmail);
                }
                else
                {
                    emailMetaData = _emailMetaData;
                }

                // create message
                var email = new MimeMessage();

                email.From.Add(MailboxAddress.Parse(emailMetaData.FromEmail));

                email.To.Add(MailboxAddress.Parse(to));

                email.Subject = subject;

                email.Body = new TextPart(TextFormat.Html) { Text = html };

                using var smtp = new SmtpClient();

                smtp.Connect(emailMetaData.SmtpServer, emailMetaData.Port, SecureSocketOptions.Auto);

                smtp.Authenticate(emailMetaData.UserName, emailMetaData.Password);

                var smtpResponse = smtp.Send(email);

                SudokuCollectiveLogger.LogInformation<EmailService>(
                    _logger,
                    LogsUtilities.GetSMTPEventId(),
                    string.Format("smptResponse: {0}", smtpResponse),
                    (Request)_requestService.Get());

                smtp.Disconnect(true);

                return true;
            }
            catch (Exception e)
            {
                SudokuCollectiveLogger.LogError<EmailService>(
                    _logger,
                    LogsUtilities.GetSMTPEventId(),
                    string.Format(LoggerMessages.ErrorThrownMessage, e.Message),
                    e,
                    (Request)_requestService.Get());
                
                return false;
            }
        }
    }
}

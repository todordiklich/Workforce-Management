using System;
using MimeKit;
using MailKit.Net.Smtp;
using System.Threading.Tasks;

using WFM.BLL.Interfaces;

namespace WFM.BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailConfiguration _emailConfiguration;

        public EmailService(IEmailConfiguration emailConfiguration)
        {
            _emailConfiguration = emailConfiguration;
        }

        public async Task SendDayOffNotificationAsync(
            string toName,
            string toEmailAddress,
            string subject,
            string message)
        {
            if (toEmailAddress == null)
            {
                return;
            }

            var email = new MimeMessage();
            //From is always the default system email from the settings file
            email.From.Add(new MailboxAddress(_emailConfiguration.SmtpNormalizedUserName, _emailConfiguration.SmtpUsername));
            email.To.Add(new MailboxAddress(toName, toEmailAddress));
            email.Subject = subject;

            var body = new BodyBuilder
            {
                HtmlBody = message
            };
            email.Body = body.ToMessageBody();

            //client setup
            var client = new SmtpClient();
            try
            {
                client.ServerCertificateValidationCallback =
                    (sender, certificate, certChainType, errors) => true;
                client.AuthenticationMechanisms.Remove("XOAUTH2");

                await client.ConnectAsync(
                    _emailConfiguration.SmtpServer,
                    _emailConfiguration.SmtpPort,
                    _emailConfiguration.SecureConnection)
                    .ConfigureAwait(_emailConfiguration.AwaitTask);

                if (_emailConfiguration.RequireAuthentication)
                {
                    await client.AuthenticateAsync(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword)
                        .ConfigureAwait(_emailConfiguration.AwaitTask);
                }

                await client.SendAsync(email).ConfigureAwait(_emailConfiguration.AwaitTask);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                await client.DisconnectAsync(true).ConfigureAwait(_emailConfiguration.AwaitTask);
                client.Dispose();
            }
        }
    }
}

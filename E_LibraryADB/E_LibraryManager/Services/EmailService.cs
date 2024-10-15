using MailKit;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using E_LibraryManager.Models;
using E_LibraryManager.ViewModels;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace E_LibraryManager.Services
{
    public class EmailService
    {
        private readonly AppSettings _appSettings;
        public EmailService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public bool SendEmail(Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            return Send(emailMessage);
        }

        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_appSettings.EmailConfiguration.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Cc.AddRange(message.Cc);
            emailMessage.Bcc.AddRange(message.Bcc);

            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(message.IsHtml ? TextFormat.Html : TextFormat.Plain) { Text = message.Content };

            return emailMessage;
        }

        private bool Send(MimeMessage mailMessage)
        {
            using (var smtpClient = new MailKit.Net.Smtp.SmtpClient())
            {
                try 
                {
                    smtpClient.Connect(_appSettings.EmailConfiguration.SmtpServer, _appSettings.EmailConfiguration.Port, true);
                    smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
                    smtpClient.ServerCertificateValidationCallback = MySslCertificateValidationCallback;
                    smtpClient.Authenticate(_appSettings.EmailConfiguration.Username, _appSettings.EmailConfiguration.Password);
                    smtpClient.MessageSent += OnMessageSent;
                    smtpClient.Send(mailMessage);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally
                {
                    smtpClient.Disconnect(true);
                }
            }
        }
        static bool MySslCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        private void OnMessageSent(object sender, MessageSentEventArgs e)
        {
            Console.WriteLine("The message was sent!");
        }

        public void SendEmailPHPProject(Email email)
        {
            SendEmail(new Message(new string[] { email.To }, email.Subject, email.Body, new string[] { }, new string[] { }, true));
        }


    }
}

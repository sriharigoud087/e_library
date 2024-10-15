using MimeKit;
using System.Text.RegularExpressions;

namespace E_LibraryManager.Models
{
    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public List<MailboxAddress> Cc { get; set; }
        public List<MailboxAddress> Bcc { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public bool IsHtml { get; set; } = true;
        public List<string> Attachments { get; set; }

        public Message(IEnumerable<string> to, string subject, string content, IEnumerable<string> cc, IEnumerable<string> bcc, bool isHtml)
        {
            To = new List<MailboxAddress>();
            Cc = new List<MailboxAddress>();
            Bcc = new List<MailboxAddress>();
            Attachments = new List<string>();

            To.AddRange(to.Where(x => validateEmailPattern(x)).Select(x => new MailboxAddress(x)));
            Cc.AddRange(cc.Where(x => validateEmailPattern(x)).Select(x => new MailboxAddress(x)));
            Bcc.AddRange(bcc.Where(x => validateEmailPattern(x)).Select(x => new MailboxAddress(x)));
            Subject = subject;
            Content = content;
        }
        private bool validateEmailPattern(string emailString)
        {
            try
            {
                return Regex.IsMatch(emailString, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}

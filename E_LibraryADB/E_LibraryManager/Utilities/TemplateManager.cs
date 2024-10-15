using System.Text;

namespace E_LibraryManager.Utilties
{
    public class EmailTemplateManager
    {
        private string GetTemplatePath(EmailTemplate emailTemplate)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory + "EmailTemplates";
            switch (emailTemplate)
            {
                case EmailTemplate.BookNotification: return $"{basePath}\\BookNotification.html";
                default:
                    return null;
            }
        }
        public string GetTemplate(EmailTemplate emailTemplate)
        {
            string path = GetTemplatePath(emailTemplate);
            if (path == null)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] b = new byte[1024];
                UTF8Encoding temp = new UTF8Encoding(true);

                while (fs.Read(b, 0, b.Length) > 0)
                {
                    sb.Append(temp.GetString(b));
                }
            }
            return sb.ToString();
        }

    }
    public enum EmailTemplate
    {
        BookNotification
    }
}

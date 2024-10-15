using E_LibraryManager.Utilties;
using E_LibraryManager.ViewModels;

namespace E_LibraryManager.Services
{
    public static class PrepareEmailBody
    {
        public static string GetEmailBody(MailBodyProperties bodyProperties)
        {
            var templateManager = new EmailTemplateManager();
            var body = templateManager.GetTemplate(EmailTemplate.BookNotification);
            body = body.Replace("##USERID", bodyProperties.UserId);
            body = body.Replace("##BOOKID", bodyProperties.BookId);
            body = body.Replace("##ISBN", bodyProperties.ISBN);
            body = body.Replace("##STATUS", bodyProperties.Status);
            body = body.Replace("##TITLE", bodyProperties.Title);
            body = body.Replace("##DUEDATE", bodyProperties.DueDate.ToString());
            body = body.Replace("##DATETAKEN", bodyProperties.DateTaken.ToString());
            body = body.Replace("##DESCRIPTION", bodyProperties.Description);

            return body;
        }
    }
}

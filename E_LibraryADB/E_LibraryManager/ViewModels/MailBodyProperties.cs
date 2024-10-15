namespace E_LibraryManager.ViewModels
{
    public class MailBodyProperties
    {
        public string UserId  { get; set; }
        public string BookId  { get; set; }
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateTaken { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
    }

}

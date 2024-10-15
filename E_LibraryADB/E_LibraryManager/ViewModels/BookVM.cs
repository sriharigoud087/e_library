namespace E_LibraryManager.ViewModels
{
    public class BookVM
    {
        public string? BookId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Image { get; set; } = null!;
        public string ISBN { get; set; } = null!;
        public string Category { set; get; } = null!;
        public int PublishedYear { get; set; }
        public string Author { get; set; } = null!;
        public bool isAvailable { get; set; }
    }

}

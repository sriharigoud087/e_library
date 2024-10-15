namespace E_LibraryManager.ViewModels
{
    public class TransactionVM {
        public string? TransactionId { get; set; } 
        public string BookId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public DateTime CheckInDateTime { set; get; }
        public DateTime CheckOutDateTime { set; get; }
        public DateTime DueDate { get; set; }
        public decimal? Penalty { get; set; }
        public string? Status { get; set; }
        public int RenewalCount { get; set; }
        public bool IsActive { set; get; }
    }
}

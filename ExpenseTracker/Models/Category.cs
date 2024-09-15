namespace ExpenseTracker.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class Expense
    {
        public int ExpenseId { get; set; }
        public string Title { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
    }
}

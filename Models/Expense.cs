using System.ComponentModel.DataAnnotations;

namespace AccountingApp.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        public string Category { get; set; } = string.Empty; // Rent, Salary, Bills

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;

        public int UserId { get; set; }
        
        public int? BillPaymentId { get; set; }
        public int? PurchaseId { get; set; }
        public int? ProductId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}

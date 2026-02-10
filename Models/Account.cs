using System.ComponentModel.DataAnnotations;

namespace AccountingApp.Models
{
    public class Account
    {
        public int Id { get; set; }

        [Required]
        public string? AccountName { get; set; }

        [Required]
        public string? AccountType { get; set; } // Cash, Bank, Expense, Income

        public decimal? Balance { get; set; } = 0;

        public int UserId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}

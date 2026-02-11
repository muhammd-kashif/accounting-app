using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountingApp.Models
{
    public class Income
    {
        public int Id { get; set; }

        [Required]
        public DateTime? Date { get; set; } = DateTime.Now;

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentType { get; set; } = string.Empty; // Cash or Bank

        public string Description { get; set; } = string.Empty;

        public int UserId { get; set; }
        
        public int? SaleId { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;
    }
}

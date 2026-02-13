using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountingApp.Models
{
    public class Sale
    {
        public int Id { get; set; }

        [Required]
        public string SaleNumber { get; set; } = string.Empty;

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [Required]
        public DateTime SaleDate { get; set; } = DateTime.Now;

        [Required]
        public string PaymentType { get; set; } = string.Empty; // Cash or Bank

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PaidAmount { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal RemainingAmount { get; set; } = 0;

        public DateTime? PaymentDueDate { get; set; }

        public bool IsPaid { get; set; } = false;

        public string? Notes { get; set; }

        public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
        public virtual ICollection<SalePayment> Payments { get; set; } = new List<SalePayment>();

        public int UserId { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;
    }

    public class SalePayment
    {
        public int Id { get; set; }

        public int SaleId { get; set; }

        [ForeignKey("SaleId")]
        public virtual Sale Sale { get; set; } = null!;

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public decimal Amount { get; set; }

        // Cash, Bank, Card
        public string PaymentMethod { get; set; } = "Cash";

        public string? ReferenceNo { get; set; } = string.Empty;

        public string? Notes { get; set; } = string.Empty;

        public int UserId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}

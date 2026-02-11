using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountingApp.Models
{
    public class Bill
    {
        public int Id { get; set; }

        public string BillNumber { get; set; } = string.Empty;

        [Required]
        public int SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; } = null!;

        public DateTime BillDate { get; set; } = DateTime.Now;

        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(30);

        // Unpaid, Partial, Paid
        public string Status { get; set; } = "Unpaid";

        public decimal TotalAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal BalanceDue => TotalAmount - PaidAmount;

        public string Notes { get; set; } = string.Empty;

        public int UserId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual ICollection<BillItem> Items { get; set; } = new List<BillItem>();
        public virtual ICollection<BillPayment> Payments { get; set; } = new List<BillPayment>();
    }

    public class BillItem
    {
        public int Id { get; set; }

        public int BillId { get; set; }

        [ForeignKey("BillId")]
        public virtual Bill Bill { get; set; } = null!;

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        public int Quantity { get; set; }

        public decimal Rate { get; set; }

        public decimal Amount { get; set; }
    }

    public class BillPayment
    {
        public int Id { get; set; }

        public int BillId { get; set; }

        [ForeignKey("BillId")]
        public virtual Bill Bill { get; set; } = null!;

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public decimal Amount { get; set; }

        // Cash, Bank, Cheque
        public string PaymentMethod { get; set; } = "Cash";

        public string ReferenceNo { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public int UserId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}

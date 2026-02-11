using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountingApp.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        [Required]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        public int SaleId { get; set; }

        [ForeignKey("SaleId")]
        public virtual Sale Sale { get; set; } = null!;

        [Required]
        public DateTime InvoiceDate { get; set; }

        public DateTime? PaymentDueDate { get; set; }

        public DateTime GeneratedDate { get; set; } = DateTime.Now;
    }
}

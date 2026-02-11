using System.ComponentModel.DataAnnotations;

namespace AccountingApp.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required]
        public string SupplierName { get; set; } = string.Empty;

        [Phone]
        public string Contact { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public decimal OpeningBalance { get; set; } = 0;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        public int UserId { get; set; }
    }
}



using System.ComponentModel.DataAnnotations;

namespace AccountingApp.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name required hai")]
        public string? Name { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Address { get; set; }

        public decimal? OpeningBalance { get; set; } = 0;

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        public int UserId { get; set; }
    }
}

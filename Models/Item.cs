using System.ComponentModel.DataAnnotations;

namespace AccountingApp.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Item name required hai")]
        public string Name { get; set; } = string.Empty;

        public string? Category { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; } = 0;

        public string? Description { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }
    }
}

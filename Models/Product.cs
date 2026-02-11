using System.ComponentModel.DataAnnotations;

namespace AccountingApp.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string ItemName { get; set; } = string.Empty;

        // Inventory, Non-Inventory, Service
        public string ItemType { get; set; } = "Inventory";

        public string SKU { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal PurchasePrice { get; set; }

        public decimal SalePrice { get; set; }

        public int StockQuantity { get; set; }

        public int ReorderLevel { get; set; } = 10;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        public int UserId { get; set; }
    }
}

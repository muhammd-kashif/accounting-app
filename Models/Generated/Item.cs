using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Models.Generated;

public partial class Item
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Category { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    [InverseProperty("Item")]
    public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}

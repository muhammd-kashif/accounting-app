using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Models.Generated;

public partial class Supplier
{
    [Key]
    public int Id { get; set; }

    public string SupplierName { get; set; } = null!;

    public string Contact { get; set; } = null!;

    public string Email { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal OpeningBalance { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int UserId { get; set; }

    public string Address { get; set; } = null!;

    [InverseProperty("Supplier")]
    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    [InverseProperty("Supplier")]
    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}

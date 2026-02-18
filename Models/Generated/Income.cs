using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Models.Generated;

[Index("CustomerId", Name = "IX_Incomes_CustomerId")]
public partial class Income
{
    [Key]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public int? CustomerId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    public string PaymentType { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int UserId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? SaleId { get; set; }

    public bool IsPaid { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("Incomes")]
    public virtual Customer? Customer { get; set; }
}

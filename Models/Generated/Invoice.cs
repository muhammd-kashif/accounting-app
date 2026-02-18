using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Models.Generated;

[Index("SaleId", Name = "IX_Invoices_SaleId")]
public partial class Invoice
{
    [Key]
    public int Id { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public int SaleId { get; set; }

    public DateTime InvoiceDate { get; set; }

    public DateTime? PaymentDueDate { get; set; }

    public DateTime GeneratedDate { get; set; }

    [ForeignKey("SaleId")]
    [InverseProperty("Invoices")]
    public virtual Sale Sale { get; set; } = null!;
}

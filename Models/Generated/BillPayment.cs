using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Models.Generated;

[Index("BillId", Name = "IX_BillPayments_BillId")]
public partial class BillPayment
{
    [Key]
    public int Id { get; set; }

    public int BillId { get; set; }

    public DateTime PaymentDate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string ReferenceNo { get; set; } = null!;

    public string Notes { get; set; } = null!;

    public int UserId { get; set; }

    public DateTime CreatedDate { get; set; }

    [ForeignKey("BillId")]
    [InverseProperty("BillPayments")]
    public virtual Bill Bill { get; set; } = null!;
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Models.Generated;

public partial class Customer
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? OpeningBalance { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int UserId { get; set; }

    public string? Address { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? CreditLimit { get; set; }

    [InverseProperty("Customer")]
    public virtual ICollection<Income> Incomes { get; set; } = new List<Income>();

    [InverseProperty("Customer")]
    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}

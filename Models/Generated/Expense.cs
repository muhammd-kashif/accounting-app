using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Models.Generated;

public partial class Expense
{
    [Key]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string Category { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    public string Description { get; set; } = null!;

    public int UserId { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? ProductId { get; set; }
}

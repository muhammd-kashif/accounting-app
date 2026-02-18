using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Models.Generated;

public partial class Account
{
    [Key]
    public int Id { get; set; }

    public string AccountName { get; set; } = null!;

    public string AccountType { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Balance { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedDate { get; set; }
}

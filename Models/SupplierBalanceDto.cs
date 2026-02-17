using System;

namespace AccountingApp.Models
{
    public class SupplierBalanceDto
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal TotalBills { get; set; } // Credit (Payable)
        public decimal TotalPaid { get; set; } // Debit (Paid)
        public decimal OpeningBalance { get; set; }
        public decimal NetBalance { get; set; } // (Opening + Bills) - Paid
        public DateTime? LastActivityDate { get; set; }
    }
}

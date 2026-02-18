using System;

namespace AccountingApp.Models
{
    public class LedgerEntry
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Debit { get; set; } // Sale Amount
        public decimal Credit { get; set; } // Payment Amount
        public decimal Balance { get; set; } // Running Balance
        public string Reference { get; set; } = string.Empty; // Sale ID or check number
        public string TransactionType { get; set; } = string.Empty; // Purchase, Bill, Payment etc.
    }
}



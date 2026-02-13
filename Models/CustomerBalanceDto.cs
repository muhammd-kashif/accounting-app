using System;

namespace AccountingApp.Models
{
    public class CustomerBalanceDto
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public decimal TotalSales { get; set; } // Debit
        public decimal TotalReceived { get; set; } // Credit
        public decimal OpeningBalance { get; set; }
        public decimal NetBalance { get; set; } // (Opening + Sales) - Received
        public decimal CreditLimit { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public int TotalOrders { get; set; }
    }
}

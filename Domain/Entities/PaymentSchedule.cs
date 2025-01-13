namespace Domain.Entities;

public class PaymentSchedule
{
        public Guid Id { get; set; } 

        
        public Guid LoanId { get; set; } 
        public Loan Loan { get; set; } 

        public DateTime PaymentDay { get; set; } 
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; } 
        public decimal RemainingAmount { get; set; } 
        public bool IsPaid { get; set; } 
 
}
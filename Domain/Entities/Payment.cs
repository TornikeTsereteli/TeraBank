namespace Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; } 
    public Loan Loan { get; set; }
    
    public DateTime PaymentDate { get; set; } 
    public decimal Amount { get; set; }

    // public bool IsLate()
    // {
    //     return PaymentDate > Loan.Client.Loans.FirstOrDefault()?.CalculateDueDate();
    // }
}
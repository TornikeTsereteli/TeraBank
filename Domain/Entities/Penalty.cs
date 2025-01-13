namespace Domain.Entities;

public class Penalty
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; }
    public decimal Amount { get; set; }
    
    public decimal RemainingAmount { get; set; }
    public string Reason { get; set; } = null!;
    public DateTime ImposedDate { get; set; }
    public bool IsPaid { get; set; } = false;
    public DateTime PaidDate { get; set; }
  
    public Loan Loan { get; set; }
}

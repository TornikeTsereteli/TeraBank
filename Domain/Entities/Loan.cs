namespace Domain.Entities;

public class Loan
{
    public Guid Id { get; set; } 
    public decimal Amount { get; set; }
    public int DurationInMonths { get; set; }
    public decimal InterestRate { get; set; } 
    public LoanStatus Status { get; set; } = LoanStatus.Pending;

    public Guid ClientId { get; set; } 
    public Client Client { get; set; }

    public IEnumerable<Payment> Payments { get; set; } = new List<Payment>();

    public decimal CalculateMonthlyPayment()
    {
        if (DurationInMonths <= 0) throw new InvalidOperationException("Duration must be greater than 0.");

        var monthlyRate = InterestRate / 100 / 12;
        return Amount * monthlyRate / (1 - (decimal) Math.Pow(1 + (double)monthlyRate, -DurationInMonths));
    }
}

public enum LoanStatus
{
    Pending,
    Approved,
    Rejected,
    Completed
}

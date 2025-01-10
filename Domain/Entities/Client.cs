namespace Domain.Entities;

public class Client
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public decimal CreditScore { get; set; }

    public IEnumerable<Loan> Loans { get; set; } = new List<Loan>();
    
    public bool IsEligibleForLoan()
    {
        var age = DateTime.Today.Year - DateOfBirth.Year;
        if (DateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
        return age >= 18; 
    }
}
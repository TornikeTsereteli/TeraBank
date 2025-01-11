using System.Collections;
using Domain.Entities;


namespace Domain.Services;

public interface ILoanService
{
    Task ApplyForLoan(Client client, Loan loan);

    public decimal CalculateMonthlyPayment(Client client)
    {
        return client.Loans.Sum(loan => CalculateConcreteLoanMonthlyPayment(client, loan));
    }

    public decimal CalculateTotalPayment(Client client)
    {
        return client.Loans.Sum(loan => CalculateConcreteLoanTotalPayment(client, loan));
    }
    
    public decimal CalculateConcreteLoanMonthlyPayment(Client client, Loan loan);
    public decimal CalculateConcreteLoanTotalPayment(Client client, Loan loan);

    public decimal CalculateConcreteLoanPenalty(Client client, Loan loan);

    public Task<IEnumerable<Loan>> GetAllLoans(Client client);
}
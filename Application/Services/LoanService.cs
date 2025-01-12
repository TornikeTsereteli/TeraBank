using System.Collections;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Services;

namespace Application.Services;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly IClientRepository _clientRepository;

    public LoanService(ILoanRepository loanRepository, IClientRepository clientRepository)
    {
        _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
    }

    public async Task ApplyForLoan(Client client, Loan loan)
    {
        if (client == null) throw new UnauthorizedAccessException("Client is unauthorized.");
        if (loan == null) throw new ArgumentNullException(nameof(loan), "Loan cannot be null.");

        if (!client.IsEligibleForLoan())
        {
            throw new InvalidOperationException("Client is not eligible for a loan.");
        }

        loan.Client = client;
        await _loanRepository.AddAsync(loan);
    }

    public decimal CalculateConcreteLoanMonthlyPayment(Client client, Loan loan)
    {
        ValidateClientLoanOwnership(client, loan);
        return loan.CalculateMonthlyPayment();
    }

    public decimal CalculateConcreteLoanTotalPayment(Client client, Loan loan)
    {
        ValidateClientLoanOwnership(client, loan);
        return loan.CalculateTotalPayment();
    }

    public decimal CalculateConcreteLoanPenalty(Client client, Loan loan)
    {
        ValidateClientLoanOwnership(client, loan);
        return loan.CalculatePenalty();
    }

    public async Task<IEnumerable<Loan>> GetAllLoans(Client client)
    {
        if (client == null) throw new UnauthorizedAccessException("Client is unauthorized.");
        return client.Loans ?? Enumerable.Empty<Loan>();
    }

    private void ValidateClientLoanOwnership(Client client, Loan loan)
    {
        if (client == null) throw new UnauthorizedAccessException("Client is unauthorized.");
        if (loan == null) throw new ArgumentNullException(nameof(loan), "Loan cannot be null.");

        if (!client.Loans.Contains(loan))
        {
            throw new ArgumentException("The specified loan does not belong to the client.");
        }
    }
}

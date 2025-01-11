using System.Collections;
using System.ComponentModel;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Services;

namespace Application.Services;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly IClientRepository _clientRepository;

    public LoanService(ILoanRepository loanRepository)
    {
        _loanRepository = loanRepository;
    }

    public async Task ApplyForLoan(Client client, Loan loan)
    {
        // should be add validations here
        if (!client.IsEligibleForLoan())
        {
            throw new ArgumentException("client is not eligible for loan");
        }
        
        if (client == null)
        {
            throw new Exception("UnAuthorized");
        }
        
        if (loan == null)
        {
            throw new ArgumentException("loan is null");
        }
        
        loan.Client = client;
        
        await _loanRepository.AddAsync(loan);
        
    }

    public decimal CalculateConcreteLoanMonthlyPayment(Client client, Loan loan)
    {
        if (!client.Loans.Contains(loan)) throw new ArgumentException("No such Loan have client");
        return loan.CalculateMonthlyPayment();
    }

    public decimal CalculateConcreteLoanTotalPayment(Client client, Loan loan)
    {
        if (!client.Loans.Contains(loan)) throw new ArgumentException("No such Loan have client");
        return loan.CalculateTotalPayment();
    }

    public decimal CalculateConcreteLoanPenalty(Client client, Loan loan)
    {
        if (!client.Loans.Contains(loan)) throw new ArgumentException("No such Loan have client");
        return loan.CalculatePenalty();
    }
    public async Task<IEnumerable<Loan>> GetAllLoans(Client client)
    {
        return client.Loans;
    }
}
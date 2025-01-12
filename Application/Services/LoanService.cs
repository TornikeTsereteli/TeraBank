using System.Collections;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Services;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<LoanService> _logger;
    private readonly ILoanApproveStrategy _loanApproveStrategy;

    public LoanService(ILoanRepository loanRepository, IClientRepository clientRepository, ILogger<LoanService> logger)
    {
        _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ApplyForLoan(Client client, Loan loan)
    {
        if (client == null)
        {
            _logger.LogError("Client is unauthorized.");
            throw new UnauthorizedAccessException("Client is unauthorized.");
        }

        if (loan == null)
        {
            _logger.LogError("Loan cannot be null.");
            throw new ArgumentNullException(nameof(loan), "Loan cannot be null.");
        }

        if (!client.IsEligibleForLoan())
        {
            _logger.LogWarning("Client with ID {ClientId} is not eligible for a loan.", client.Id);
            throw new InvalidOperationException("Client is not eligible for a loan.");
        }

        _logger.LogInformation("Applying for loan for client with ID {ClientId}.", client.Id);
      
        loan.Client = client;
        loan.Status = _loanApproveStrategy.isLoanApproved(client, loan) ? LoanStatus.Approved : LoanStatus.Rejected;
        
        await _loanRepository.AddAsync(loan);
        _logger.LogInformation("Loan applied successfully for client with ID {ClientId}. Loan ID: {LoanId}", client.Id, loan.Id);
    }

    public decimal CalculateConcreteLoanMonthlyPayment(Client client, Loan loan)
    {
        ValidateClientLoanOwnership(client, loan);
        decimal monthlyPayment = loan.CalculateMonthlyPayment();
        _logger.LogInformation("Calculated monthly payment for loan with ID {LoanId}: {MonthlyPayment}", loan.Id, monthlyPayment);
        return monthlyPayment;
    }

    public decimal CalculateConcreteLoanTotalPayment(Client client, Loan loan)
    {
        ValidateClientLoanOwnership(client, loan);
        decimal totalPayment = loan.CalculateTotalPayment();
        _logger.LogInformation("Calculated total payment for loan with ID {LoanId}: {TotalPayment}", loan.Id, totalPayment);
        return totalPayment;
    }

    public decimal CalculateConcreteLoanPenalty(Client client, Loan loan)
    {
        ValidateClientLoanOwnership(client, loan);
        decimal penalty = loan.CalculatePenalty();
        _logger.LogInformation("Calculated penalty for loan with ID {LoanId}: {Penalty}", loan.Id, penalty);
        return penalty;
    }

    public async Task<IEnumerable<Loan>> GetAllLoans(Client client)
    {
        if (client == null)
        {
            _logger.LogError("Client is unauthorized.");
            throw new UnauthorizedAccessException("Client is unauthorized.");
        }

        _logger.LogInformation("Fetching all loans for client with ID {ClientId}.", client.Id);
        var loans = client.Loans ?? Enumerable.Empty<Loan>();
        _logger.LogInformation("Found {LoanCount} loan(s) for client with ID {ClientId}.", loans.Count(), client.Id);
        return loans;
    }

    private void ValidateClientLoanOwnership(Client client, Loan loan)
    {
        if (client == null)
        {
            _logger.LogError("Client is unauthorized.");
            throw new UnauthorizedAccessException("Client is unauthorized.");
        }

        if (loan == null)
        {
            _logger.LogError("Loan cannot be null.");
            throw new ArgumentNullException(nameof(loan), "Loan cannot be null.");
        }

        if (!client.Loans.Contains(loan))
        {
            _logger.LogWarning("Client with ID {ClientId} does not own the specified loan with ID {LoanId}.", client.Id, loan.Id);
            throw new ArgumentException("The specified loan does not belong to the client.");
        }
    }
}

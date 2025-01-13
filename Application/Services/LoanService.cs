using System.Collections;
using Application.DTOs;
using Application.Mappers;
using Domain.Entities;
using Domain.Helpers;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Services;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly ILogger<LoanService> _logger;
    private readonly ILoanApproveStrategy _loanApproveStrategy;
    private readonly IPaymentScheduleRepository _paymentScheduleRepository;
    private readonly IUnitOfWork _unitOfWork; // I just Use this for transaction

    public LoanService(ILoanRepository loanRepository, ILogger<LoanService> logger, ILoanApproveStrategy loanApproveStrategy, IPaymentScheduleRepository paymentScheduleRepository, IUnitOfWork unitOfWork)
    {
        _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _loanApproveStrategy = loanApproveStrategy;
        _paymentScheduleRepository = paymentScheduleRepository;
        _unitOfWork = unitOfWork;
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
        _logger.LogInformation("the transaction Start");
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            loan.Client = client;
            loan.Status = _loanApproveStrategy.isLoanApproved(client, loan) ? LoanStatus.Approved : LoanStatus.Rejected;
        
            await _loanRepository.AddAsync(loan);
            IEnumerable<PaymentSchedule> paymentSchedules = loan.GeneratePaymentSchedules();
            await _paymentScheduleRepository.AddPaymentSchedulesAsync(paymentSchedules);
            
            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Loan applied successfully for client with ID {ClientId}. Loan ID: {LoanId}", client.Id, loan.Id);
            _logger.LogInformation("transaction successfully commited");
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    

    private void ValidateClientLoanOwnership(Client? client, Loan? loan)
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

    public async Task<Object> GetLoanStatus(Client client, Guid id)
    {
        Loan? loan = await _loanRepository.GetByIdWithPaymentScheduleAsync(id);
        
        ValidateClientLoanOwnership(client,loan);
        
        return new LoanResponseDTO()
        {
            LoanId = loan.Id,
            MonthlyPayment = loan.CalculateMonthlyPayment(),
            CurrentMonthPayment = loan.GetThisMonthPayment(),
            RemainingAmount = loan.RemainingAmount,
            Name = loan.Name,
            StartDate = loan.StartDate,
            EndDate = loan.EndDate,
            Status = loan.Status.ToDisplayString()
        };
    }
}

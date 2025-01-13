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

    /// <summary>
    /// Applies for a loan for the specified client, including eligibility checks, loan approval, and transaction management.
    /// </summary>
    /// <remarks>
    /// This method handles the entire process of applying for a loan, including:
    /// 1. Checking the client's eligibility.
    /// 2. Determining the loan's status using a loan approval strategy.
    /// 3. Adding the loan to the database.
    /// 4. Generating a payment schedule for the loan.
    /// 5. Saving the payment schedule to the database.
    /// 6. Ensuring that the operation is atomic by using a transaction that either commits all changes or rolls back in case of failure.
    /// </remarks>
    /// <param name="client">The client applying for the loan.</param>
    /// <param name="loan">The loan being applied for by the client.</param>
    /// <exception cref="UnauthorizedAccessException">Thrown when the client is not authorized to apply for a loan.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the provided loan is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the client is not eligible for a loan.</exception>
    /// <returns>A task representing the asynchronous operation.</returns>

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
            _logger.LogError(e.Message);
            throw;
        }
    }

    
    // validation of client and loan, and checks if loan belongs to client.Loans
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
    
    // firstly loan and user are validated ,
    // then LoanResponse is returned which contains what is remaining amount of this month, and updated MonthlyPayment if overpayment occurs
    
    public async Task<Object> GetLoanStatus(Client client, Guid id)
    {
        Loan? loan = await _loanRepository.GetByIdWithPaymentScheduleAsync(id);
        
        ValidateClientLoanOwnership(client,loan);
        
        return new LoanResponseDTO()
        {
            LoanId = loan.Id,
            MonthlyPayment = loan.GetNextMonthPayment(),
            CurrentMonthPayment = loan.GetThisMonthPayment(),
            RemainingAmount = loan.RemainingAmount,
            Name = loan.Name,
            StartDate = loan.StartDate,
            EndDate = loan.EndDate,
            Status = loan.Status.ToDisplayString()
        };
    }
}

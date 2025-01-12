using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Services;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IPenaltyRepository _penaltyRepository;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IPaymentRepository paymentRepository, ILoanRepository loanRepository, IPenaltyRepository penaltyRepository, ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        _penaltyRepository = penaltyRepository ?? throw new ArgumentNullException(nameof(penaltyRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task MakePayment(int amount, Guid loanId)
    {
        if (amount <= 0)
        {
            _logger.LogWarning("Invalid payment amount: {Amount}. Amount must be greater than zero.", amount);
            throw new ArgumentException("Payment amount must be greater than zero.");
        }
        
        Loan loan = await _loanRepository.GetByIdAsync(loanId);
        if (loan == null)
        {
            _logger.LogError("Loan with ID {LoanId} not found.", loanId);
            throw new InvalidOperationException("Loan not found.");
        }
        
        if (loan.Status == LoanStatus.Completed)
        {
            _logger.LogWarning("Attempted to make a payment on a completed loan. Loan ID: {LoanId}", loanId);
            throw new InvalidOperationException("Cannot make payments on a completed loan.");
        }
        
        if (loan.Status == LoanStatus.Rejected)
        {
            _logger.LogWarning("Attempted to make a payment on a rejected loan. Loan ID: {LoanId}", loanId);
            throw new InvalidOperationException("Cannot make payments on a rejected loan.");
        }

        if (amount > loan.RemainingAmount)
        {
            _logger.LogWarning("Payment amount exceeds the remaining loan amount. Loan ID: {LoanId}, Payment Amount: {Amount}, Remaining Amount: {RemainingAmount}",
                loanId, amount, loan.RemainingAmount);
            throw new InvalidOperationException("Payment amount exceeds the remaining loan amount.");
        }

        var payment = new Payment
        {
            Amount = amount,
            PaymentDate = DateTime.Now,
            Loan = loan
        };
        
        _logger.LogInformation("Processing payment of amount {Amount} for loan ID {LoanId}.", amount, loanId);

        loan.RemainingAmount -= amount;

        if (loan.RemainingAmount <= 0)
        {
            loan.RemainingAmount = 0; 
            loan.Status = LoanStatus.Completed;
            _logger.LogInformation("Loan ID {LoanId} is completed after payment. Remaining Amount: {RemainingAmount}.", loanId, loan.RemainingAmount);
        }

        try
        {
            await _paymentRepository.AddAsync(payment);
            await _loanRepository.UpdateAsync(loan);
            _logger.LogInformation("Payment of amount {Amount} for loan ID {LoanId} was successfully processed.", amount, loanId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing payment for loan ID {LoanId}.", loanId);
            throw new InvalidOperationException("An error occurred while processing the payment.", ex);
        }
    }

    public async Task MakePenaltyPayment(int amount, Guid penaltyId)
    {
        Penalty penalty = await _penaltyRepository.GetByIdAsync(penaltyId);
        if (penalty == null)
        {
            _logger.LogError("Penalty with ID {PenaltyId} not found.", penaltyId);
            throw new ArgumentException("No such penalty exists");
        }

        if (penalty.IsPaid)
        {
            _logger.LogWarning("Penalty with ID {PenaltyId} is already paid.", penaltyId);
            throw new ArgumentException("Penalty is already paid");
        }

        penalty.RemainingAmount -= amount;
        if (penalty.RemainingAmount <= 0)
        {
            penalty.IsPaid = true;
            _logger.LogInformation("Penalty with ID {PenaltyId} has been fully paid. Remaining Amount: {RemainingAmount}.", penaltyId, penalty.RemainingAmount);
            penalty.RemainingAmount = 0;
        }

        await _penaltyRepository.UpdateAsync(penalty);
        _logger.LogInformation("Penalty payment of amount {Amount} for penalty ID {PenaltyId} processed successfully.", amount, penaltyId);
    }

    public async Task<IEnumerable<Payment>> GetAllPayments(Client client)
    {
        _logger.LogInformation("Fetching all payments for client with ID {ClientId}.", client.Id);
        var payments = client.Loans.Select(loan => loan.Payments).Aggregate(new List<Payment>(), (acc, loan) =>
        {
            acc.AddRange(loan);
            return acc;
        });

        _logger.LogInformation("Found {PaymentCount} payment(s) for client with ID {ClientId}.", payments.Count(), client.Id);
        return payments;
    }
}

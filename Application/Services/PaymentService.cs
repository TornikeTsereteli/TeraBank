using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Services;
using Infrastructure.Database;
using Microsoft.Extensions.Logging;

namespace Application.Services;


///  this class responsibility is to manage payment service, pay loan, pay penalty, ..... , get all payments, I hope Single responsibility principle is not violated 
public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IPenaltyRepository _penaltyRepository;
    private readonly ILogger<PaymentService> _logger;
    private readonly IUnitOfWork _unitOfWork; // same, just use for the transaction
    private readonly IMoneySentBackStrategy _moneySentBack; // money sent back strategy.

    public PaymentService(IPaymentRepository paymentRepository, ILoanRepository loanRepository, IPenaltyRepository penaltyRepository, ILogger<PaymentService> logger, IUnitOfWork unitOfWork, IMoneySentBackStrategy moneySentBack)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        _penaltyRepository = penaltyRepository ?? throw new ArgumentNullException(nameof(penaltyRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _unitOfWork = unitOfWork;
        _moneySentBack = moneySentBack;
    }
    
    
    /// <summary>
    /// Processes a payment for a specified loan, ensuring that  validations are performed before proceeding with the payment.
    /// The method first validates the payment amount, checks the loan status, and ensures that the payment amount does not exceed the remaining loan balance.
    /// If any validation fails, appropriate exceptions are thrown. If all validations pass, the payment is recorded, the loan is updated, paymentSchedules are updated
    /// and changes are committed to the database within a transaction.
    /// </summary>
    /// <param name="amount">The payment amount to be made. It must be greater than zero and cannot exceed the remaining loan balance.</param>
    /// <param name="loanId">The unique identifier of the loan for which the payment is being made. The loan must exist in the system and be in a valid state for payment.</param>
    /// <exception cref="ArgumentException">Thrown when the payment amount is less than or equal to zero.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when:
    /// - The loan with the specified ID cannot be found.
    /// - The loan status is "Completed" (i.e., payments are no longer accepted).
    /// - The loan status is "Rejected" (i.e., payments are not accepted).
    /// </exception>
    /// <remarks>
    ///in case of overpayment the loan remaiing amount is less than amount then sent back to the client with  _moneySentBackStrategy magic
    /// 
    /// This method uses a transaction to ensure that the payment is successfully processed and the loan record is updated atomically.
    /// In case of an error during processing, the transaction is rolled back to prevent partial updates to the database.
    /// The method also logs various events such as validation failures, transaction start, success, and errors to facilitate monitoring and troubleshooting.
    /// </remarks>
    public async Task MakePayment(decimal amount, Guid loanId)
    {
        if (amount <= 0)
        {
            _logger.LogWarning("Invalid payment amount: {Amount}. Amount must be greater than zero.", amount);
            throw new ArgumentException("Payment amount must be greater than zero.");
        }
        
        Loan? loan = await _loanRepository.GetByIdWithPaymentScheduleAsync(loanId);
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
            _logger.LogInformation("amount exceeds remainingMoney");
            await _moneySentBack.SentBack(amount - loan.RemainingAmount);  // exceed money back to the account
            amount = loan.RemainingAmount; // amount is updated 
        }

        var payment = new Payment
        {
            Amount = amount,
            PaymentDate = DateTime.Now,
            Loan = loan
        };
        
        _logger.LogInformation("Processing payment of amount {Amount} for loan ID {LoanId}.", amount, loanId);
        
        try
        {
            _logger.LogInformation("Begin Transaction");
            await _unitOfWork.BeginTransactionAsync();
            
            loan.MakePayment(amount); // changes paymentSchedules, handle overpayment successfully 
            await _paymentRepository.AddAsync(payment); // payment is added
            await _loanRepository.UpdateAsync(loan); //  loan is updated
           
            _logger.LogInformation("Payment of amount {Amount} for loan ID {LoanId} was successfully processed.", amount, loanId);
            await _unitOfWork.CommitAsync(); // all changes are commited to the db 
            _logger.LogWarning("commit transaction");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogInformation("transaction rollbacked");
            _logger.LogError(ex, "Error occurred while processing payment for loan ID {LoanId}.", loanId);
            throw new InvalidOperationException("An error occurred while processing the payment.", ex);
        }
    }
    
    // just simple penalty payment , case of overpayment is handled successfully as it was handled in makepayment method 
    public async Task MakePenaltyPayment(decimal amount, Guid penaltyId)
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
            if (penalty.RemainingAmount < 0)
            {
                await _moneySentBack.SentBack(-penalty.RemainingAmount);  // money is sent back to the user account
            }
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

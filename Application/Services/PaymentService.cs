using Domain.Entities;
using Domain.Interfaces;
using Domain.Services;

namespace Application.Services;

public class PaymentService : IPaymentService
{

    private readonly IPaymentRepository _paymentRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IPenaltyRepository _penaltyRepository;

    public PaymentService(IPaymentRepository paymentRepository, ILoanRepository loanRepository)
    {
        _paymentRepository = paymentRepository;
        _loanRepository = loanRepository;
    }

    public async Task MakePayment(int amount, Guid loanId)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Payment amount must be greater than zero.");
        }
        
        Loan loan = await _loanRepository.GetByIdAsync(loanId);
        if (loan == null)
        {
            throw new InvalidOperationException("Loan not found.");
        }
        
        if (loan.Status == LoanStatus.Completed)
        {
            throw new InvalidOperationException("Cannot make payments on a completed loan.");
        }
        
        if (loan.Status == LoanStatus.Rejected)
        {
            throw new InvalidOperationException("Cannot make payments on a rejected loan.");
        }

        if (amount > loan.RemainingAmount)
        {
            throw new InvalidOperationException("Payment amount exceeds the remaining loan amount.");
        }

        var payment = new Payment
        {
            Amount = amount,
            PaymentDate = DateTime.Now,
            Loan = loan
        };
        

        loan.RemainingAmount -= amount;

        if (loan.RemainingAmount <= 0)
        {
            loan.RemainingAmount = 0; 
            loan.Status = LoanStatus.Completed;
        }

        try
        {
            await _paymentRepository.AddAsync(payment);
            await _loanRepository.UpdateAsync(loan);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred while processing payment: {ex.Message}");
            throw new InvalidOperationException("An error occurred while processing the payment.", ex);
        }
    }


    public async Task MakePenaltyPayment(int amount, Guid penaltyId)
    {
        Penalty penalty = await _penaltyRepository.GetByIdAsync(penaltyId);
        if (penalty == null)
        {
            throw new ArgumentException("No such penalty exists");
        }

        if (penalty.IsPaid)
        {
            throw new ArgumentException("penalty is already Paid");
        }

        penalty.RemainingAmount -= amount;
        if (penalty.RemainingAmount <= 0)
        {
            penalty.IsPaid = true;
            // send money back  some logic can be added 
            penalty.RemainingAmount = 0;
        }

        await _penaltyRepository.UpdateAsync(penalty);

    }


    
    
    public async Task<IEnumerable<Payment>> GetAllPayments(Client client)
    {
        return client.Loans.Select(loan => loan.Payments).Aggregate(new List<Payment>(), (acc, loan) =>
        {
            acc.AddRange(loan);
            return acc;
        });
    }
}
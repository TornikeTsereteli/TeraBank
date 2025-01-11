using Domain.Entities;
using Domain.Interfaces;
using Domain.Services;

namespace Application.Services;

public class PaymentService : IPaymentService
{

    private IPaymentRepository _paymentRepository;
    private ILoanRepository _loanRepository;

    public PaymentService(IPaymentRepository paymentRepository, ILoanRepository loanRepository)
    {
        _paymentRepository = paymentRepository;
        _loanRepository = loanRepository;
    }

    public async Task MakePayment(int amount, Guid loanId)
    {
        Loan loan = await _loanRepository.GetByIdAsync(loanId);
        //
        // if (await HasThisMonthCovered(loan))
        // {
        //     throw new ArgumentException("This Month is AlreadyCovered");
        // }
        Payment payment = new Payment()
        {
            Amount = amount,
            PaymentDate = DateTime.Now,
            Loan = loan
        };
        
        loan.RemainingAmount -= amount;
        
        await _loanRepository.UpdateAsync(loan);
        await _paymentRepository.AddAsync(payment);
        
    }



    private async Task<bool> HasThisMonthPaymentCovered(Loan loan)
    {
        var monthlyPayment = loan.CalculateMonthlyPayment();
        decimal thisMonthPaymentsSum = new decimal(0.5);
        await Task.CompletedTask;
        return monthlyPayment <= thisMonthPaymentsSum;
    }

    
    public async Task GetAllPayments(Client client)
    {
        
    }
}
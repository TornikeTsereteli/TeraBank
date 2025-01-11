using System.ComponentModel;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Application.Services;

public class PaymentCheckService : BackgroundService
{
    private readonly ILoanRepository _loanRepository;

    public PaymentCheckService(ILoanRepository loanRepository)
    {
        _loanRepository = loanRepository;
    }


    private async Task CheckPayments(CancellationToken stoppingToken)
    {
        var loans =await _loanRepository.GetAllAsync();
        var todaysDedlineLoans = loans.Where(x => IsPaymentDay(x.StartDate));
        
        
        
        foreach (var loan in todaysDedlineLoans)
        {
            if (!HasPaidThisMonth(loan))
            {
                // apply Penalty
                Console.WriteLine("apply penalty");
            }
        }
        
    }

    private bool IsPaymentDay(DateTime startDate)
    {
        var today = DateTime.Now;

        return startDate.Day == today.Day && startDate <= today;
    }

    private bool HasPaidThisMonth(Loan loan)
    {
        var now = DateTime.Now;
        return loan.Payments.Sum(payment => payment.Amount) >= loan.CalculateMonthlyPayment();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

            await CheckPayments(stoppingToken);
        }
    }
}
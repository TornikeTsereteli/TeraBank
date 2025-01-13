using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class PaymentCheckService : BackgroundService
{
    private readonly ILoanRepository _loanRepository;
    private readonly IPenaltyRepository _penaltyRepository;
    private readonly ILogger<PaymentCheckService> _logger;

    public PaymentCheckService(
        ILoanRepository loanRepository,
        IPenaltyRepository penaltyRepository,
        ILogger<PaymentCheckService> logger)
    {
        _loanRepository = loanRepository;
        _penaltyRepository = penaltyRepository;
        _logger = logger;
    }

    private async Task CheckPayments(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting payment checks...");

        var loans = await _loanRepository.GetAllAsync();

        foreach (var loan in loans)
        {
            if (!loan.HaveLastMonthsFullyPaid())
            {
                var penalty = new Penalty
                {
                    Amount = loan.CalculatePenalty(),
                    Reason = "Late payment penalty",
                    ImposedDate = DateTime.Now,
                    IsPaid = false,
                    Loan = loan
                };

                // email sending logic can be be added
                
                await _penaltyRepository.AddAsync(penalty);
            }
        }

        _logger.LogInformation("Payment checks completed.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PaymentCheckService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                await CheckPayments(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during payment checks.");
            }
        }

        _logger.LogInformation("PaymentCheckService is stopping.");
    }
}

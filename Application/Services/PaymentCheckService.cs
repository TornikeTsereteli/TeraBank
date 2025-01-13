using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


// BackGrounds Service Class Which is responsible to Check every day for every client of somene desrves penalty fee, tu ki gamoweros jarima
public class PaymentCheckService : BackgroundService
{
    private readonly ILogger<PaymentCheckService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public PaymentCheckService(
        ILogger<PaymentCheckService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    private async Task CheckPayments(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var _loanRepository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
        var _penaltyRepository = scope.ServiceProvider.GetRequiredService<IPenaltyRepository>();
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

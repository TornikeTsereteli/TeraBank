using Domain.Entities;
using Domain.Interfaces.Repositories;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PaymentScheduleRepository : IPaymentScheduleRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentScheduleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddPaymentSchedulesAsync(IEnumerable<PaymentSchedule> schedules)
    {
        foreach (var schedule in schedules)
        {
            // var loan = await _context.Loans.Include(l => l.PaymentSchedules)
            //     .FirstOrDefaultAsync(l => l.Id == schedule.LoanId);
        
            // if (loan != null)
            // {
            //     loan.PaymentSchedules.Add(schedule); // Adding the schedule to the Loan entity
            // }

            await _context.PaymentSchedules.AddAsync(schedule); // Add the schedule to the context
        }

        await _context.SaveChangesAsync();
    }
}
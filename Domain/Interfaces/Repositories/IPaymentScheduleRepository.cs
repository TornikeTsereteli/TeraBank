using Domain.Entities;

namespace Domain.Interfaces.Repositories;

public interface IPaymentScheduleRepository
{
    Task AddPaymentSchedulesAsync(IEnumerable<PaymentSchedule> schedules);
}
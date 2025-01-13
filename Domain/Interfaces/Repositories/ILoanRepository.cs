using Domain.Entities;

namespace Domain.Interfaces.Repositories;

public interface ILoanRepository : IGeneralRepository<Loan>
{
    public Task<Loan?> GetByIdWithPaymentScheduleAsync(Guid id);
}
using Domain.Interfaces.Repositories;

namespace Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IClientRepository _clientRepository { get; set; }
    ILoanRepository _loanRepository { get; set; }
    IPaymentRepository _paymentRepository { get; set; }
    IPenaltyRepository _penaltyRepository { get; set; }

    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
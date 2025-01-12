using System.Data;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{

    private readonly ApplicationDbContext _context;

    private IDbContextTransaction _currentTransaction;
    
    public IClientRepository _clientRepository { get; set; }
    public ILoanRepository _loanRepository { get; set; }
    public IPaymentRepository _paymentRepository { get; set; }
    public IPenaltyRepository _penaltyRepository { get; set; }
    

    public UnitOfWork(IClientRepository clientRepository, ILoanRepository loanRepository, IPaymentRepository paymentRepository, IPenaltyRepository penaltyRepository, ApplicationDbContext context)
    {
        _clientRepository = clientRepository;
        _loanRepository = loanRepository;
        _paymentRepository = paymentRepository;
        _penaltyRepository = penaltyRepository;
        _context = context;
    }

    public async Task BeginTransactionAsync()
    {
        _currentTransaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_currentTransaction != null)
        {
            try
            {
                await _context.SaveChangesAsync();
                await _currentTransaction.CommitAsync();
            }
            catch (Exception)
            {
                await RollbackAsync();
                throw;
            }
        }
    }

    public async Task RollbackAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync();
        }
    }


    public void Dispose()
    {
        _context.Dispose();
        _currentTransaction.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        await _currentTransaction.DisposeAsync();
    }
}
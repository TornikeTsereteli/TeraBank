using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

// same Loan approvment strategy class, if someone want to change the strategy he can write new class and inject it 
public class LoanApproveStrategy : ILoanApproveStrategy
{
    public bool isLoanApproved(Client client, Loan loan)
    {
        return true;
    }
}
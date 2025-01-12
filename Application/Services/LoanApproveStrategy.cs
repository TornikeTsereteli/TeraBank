using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class LoanApproveStrategy : ILoanApproveStrategy
{
    public bool isLoanApproved(Client client, Loan loan)
    {
        return true;
    }
}
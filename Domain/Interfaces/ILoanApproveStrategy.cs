using Domain.Entities;

namespace Domain.Interfaces;

public interface ILoanApproveStrategy
{
    bool isLoanApproved(Client client, Loan loan);
}
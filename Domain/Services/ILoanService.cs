using System.Collections;
using Domain.Entities;


namespace Domain.Services;

public interface ILoanService
{
    Task ApplyForLoan(Client client, Loan loan);
    
    Task<Object> GetLoanStatus(Client client, Guid id);
}
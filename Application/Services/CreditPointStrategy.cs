using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class CreditPointStrategy : ICreditPointStrategy
{
    public int GetCreditPoint()
    {
        return 600;
    }
}
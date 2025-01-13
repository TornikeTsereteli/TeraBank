using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;


// stategy class, intended for Credit Point assign strategy this is just Moq class
public class CreditPointStrategy : ICreditPointStrategy
{
    public int GetCreditPoint()
    {
        return 600;
    }
}
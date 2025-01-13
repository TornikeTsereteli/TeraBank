using Application.DTOs;
using Domain.Entities;
using Domain.Helpers;

namespace Application.Mappers;

public static class Mapper
{
    public static LoanResponseDTO LoanToLoanResponseDto(Loan loan)
    {
        return new LoanResponseDTO()
        {
            LoanId = loan.Id,
            MonthlyPayment = loan.CalculateMonthlyPayment(),
            Name = loan.Name,
            RemainingAmount = loan.RemainingAmount,
            StartDate = loan.StartDate,
            EndDate = loan.EndDate,
            Status = loan.Status.ToDisplayString()
        };
    }

    public static PenaltyResponseDTO PenaltyToPenaltyResponseDto(Penalty penalty)
    {
        return new PenaltyResponseDTO()
        {
            Id = penalty.Id,
            Amount = penalty.Amount,
            RemainingAmount = penalty.RemainingAmount,
            IsPaid = penalty.IsPaid,
            Reason = penalty.Reason,
            ImposedDate = penalty.ImposedDate,
            PaidDate = penalty.PaidDate
        };
    }
}
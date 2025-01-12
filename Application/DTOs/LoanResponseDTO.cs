using Domain.Entities;

namespace Application.DTOs;

public class LoanResponseDTO
{
    public Guid LoanId { get; set; }
    public LoanStatus Status { get; set; }
    public decimal MonthlyPayment { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
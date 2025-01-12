using Domain.Entities;

namespace Application.DTOs;

public class LoanStatusDTO
{
    public Guid LoanId { get; set; }
    public LoanStatus Status { get; set; }
    public decimal MonthlyPayment { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal Penalty { get; set; }
    public DateTime NextPaymentDue { get; set; }
    public int PaymentsCompleted { get; set; }
}
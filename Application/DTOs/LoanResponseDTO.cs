using Domain.Entities;

namespace Application.DTOs;

public class LoanResponseDTO
{
    public Guid LoanId { get; set; }
    
    public string Name { get; set; }
    
    public decimal RemainingAmount { get; set; }
    public string Status { get; set; }
    public decimal MonthlyPayment { get; set; }
    
    public decimal CurrentMonthPayment { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
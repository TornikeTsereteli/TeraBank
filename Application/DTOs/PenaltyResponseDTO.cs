using System.Runtime.InteropServices.JavaScript;

namespace Application.DTOs;

public class PenaltyResponseDTO
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public decimal RemainingAmount { get; set; }
    public bool IsPaid { get; set; }
    public string Reason { get; set; }
    public DateTime ImposedDate { get; set; }
    public DateTime PaidDate { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class PaymentDTO
{
    [Required]
    public int Amount { get; set; }
    
    [Required]
    public Guid LoanId { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class PaymentDTO
{
    [Required]
    [Range(1,100000)]
    public decimal Amount { get; set; }
    
    [Required]
    public Guid LoanId { get; set; }
}
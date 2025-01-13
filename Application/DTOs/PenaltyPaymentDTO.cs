using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class PenaltyPaymentDTO
{
    [Required]
    public Guid PenaltyID { get; set; }
    
    [Required]
    [Range(0,100000)]
    public decimal Amount { get; set; }
    
}
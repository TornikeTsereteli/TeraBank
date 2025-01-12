using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class PenaltyPaymentDTO
{
    [Required]
    public Guid PenaltyID { get; set; }
    
    [Required]
    public int Amount { get; set; }
    
}
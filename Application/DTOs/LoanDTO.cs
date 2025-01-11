using System.ComponentModel.DataAnnotations;
using Domain.Entities;

namespace Application.DTOs;

public class LoanDTO
{

    [Required]
    public string Name { get; set; }
    
    [Required]
    public int Amount { get; set; }
    
    [Required]
    public int InterestRate { get; set; }
    
    [Required]
    public int DurationInMonth { get; set; }
    
}
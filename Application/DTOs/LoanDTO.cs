using System.ComponentModel.DataAnnotations;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.DTOs;

public class LoanDTO
{

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }

    [Required]
    [Range(100, 1000000)] 
    public decimal Amount { get; set; }

    [Required]
    [Range(0, 50)]
    [Precision(2)] 
    public decimal InterestRate { get; set; }

    
    [Required]
    [Range(1, 240)] 
    public int DurationInMonth { get; set; }

    
}
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class RegisterDTO
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string FirstName { get; set; }
    
    [Required]
    public string LastName { get; set; }
    
    [Required]
    public DateTime DateOfBirth { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    
}
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public class AppUser : IdentityUser
{
    public Guid ClientId { get; set; }
    
    public Client Client { get; set; }
}
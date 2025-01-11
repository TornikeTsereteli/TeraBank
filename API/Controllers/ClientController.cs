using System.Security.Claims;
using Domain.Entities;
using Domain.Services;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientController : ControllerBase
{
    private readonly ILoanService _loanService;
    private readonly UserManager<AppUser> _userManager;


    public ClientController(ILoanService loanService, UserManager<AppUser> userManager)
    {
        _loanService = loanService;
        _userManager = userManager;
    }

    [HttpGet("details")]
    public async Task<IActionResult> GetClientDetails()
    {
        return Ok();
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetClientLoanHistory()
    {
        Client client = await GetClient();
        return Ok(client.Loans.Select(x=> x.RemainingAmount));
    }
    
    
    private async Task<Client?> GetClient()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.Users
            .Include(u => u.Client)
            .ThenInclude(c=>c.Loans)
            .FirstOrDefaultAsync(u => u.Id == userId);
        return user?.Client;
    }
}
using System.Security.Claims;
using Application.DTOs;
using Domain.Entities;
using Domain.Services;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoanController : ControllerBase
{

    private readonly ILoanService _loanService;
    private readonly UserManager<AppUser> _userManager;

    public LoanController(ILoanService loanService, UserManager<AppUser> userManager)
    {
        _loanService = loanService;
        _userManager = userManager;
    }

    [HttpPost("apply")]
    public async Task<IActionResult> ApplyForLoan([FromBody] LoanDTO loanDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("errors");
        }
        Client? client = await GetClient();
        if (client == null)
        {
            return BadRequest("UnAuthorized");
        }
        if (!client.IsEligibleForLoan())
        {
            return BadRequest("client is not allow to have loan due to age permissions");
        }
        Loan loan = new Loan()
        {
            Amount = loanDto.Amount,
            InterestRate = loanDto.InterestRate,
            DurationInMonths = loanDto.DurationInMonth,
            Status = LoanStatus.Approved
        };
        
        await _loanService.ApplyForLoan(client, loan);
        
        return Ok("Loan is Approved");
    }

    [HttpGet("status/{id}")]
    public async Task<IActionResult> GetLoanStatus(Guid id)
    {
        var client = await GetClient();
        if (client == null)
        {
            return StatusCode(401, "UnAuthorized");
        }

        // var status = client?.Loans.FirstOrDefault(l=>l.Id.Equals(id))?.Status;
        var status = client?.Loans.Select(l=>l.Status);
        if (status == null)
        {
            return BadRequest("no Such Loan have this client");
        }
        Console.WriteLine(status);
        return Ok(status);
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
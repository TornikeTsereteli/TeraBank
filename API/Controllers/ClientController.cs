using System.Security.Claims;
using API.Response;
using Domain.Entities;
using Domain.Services;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Application.DTOs;

namespace API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ClientController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly ILogger<ClientController> _logger;

    public ClientController(IClientService clientService, ILogger<ClientController> logger)
    {
        _clientService = clientService;
        _logger = logger;
    }

    [HttpGet("loan-history")]
    public async Task<IActionResult> GetClientLoanHistory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new ApiResponse(401, "User is not authenticated."));
        }

        var client = await _clientService.GetClientByUserIdWithLoansAsync(userId);
        if (client == null)
        {
            return NotFound(new ApiResponse(404, "Client not found."));
        }

        if (client.Loans == null || !client.Loans.Any())
        {
            return Ok(new ApiResponse(200, "No loan history available.", new { loans = Enumerable.Empty<decimal>() }));
        }

        var loanHistory = client.Loans.Select(x => new
        {
            Id = x.Id,
            Amount = x.Amount,
            RemainingAmount = x.RemainingAmount,
            Status = x.Status,
            StartDate = x.StartDate,
            EndDate = x.EndDate
        }).ToList();

        return Ok(new ApiResponse(200, "Loan history retrieved successfully.", new { loans = loanHistory }));
    }

    [HttpGet("penalty-fees")]
    public async Task<IActionResult> GetPenaltyFees()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var client = await _clientService.GetClientByUserIdWithLoansAndPenalties(userId);
        if (client == null)
        {
            return Unauthorized();
        }
        
        var fees = client.Loans.Select(loan => loan.Penalties).Aggregate(new List<Penalty>(), (acc, penalties) =>
        {
            acc.AddRange(penalties);
            return acc;
        }).Select(x => new 
        {
            x.Id,
            x.Amount,
            x.RemainingAmount,
            x.IsPaid,
            x.Reason,
            x.ImposedDate,
            x.PaidDate
        });

        return Ok(new ApiResponse(200, "your penalty Fees are", fees));
    }
}

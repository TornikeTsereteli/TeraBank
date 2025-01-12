using System.Security.Claims;
using API.Response;
using Application.DTOs;
using Domain.Entities;
using Domain.Helpers;
using Domain.Services;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class LoanController : ControllerBase
{
    private readonly ILoanService _loanService;
    private readonly ILogger<LoanController> _logger;
    private readonly IClientService _clientService;

    public LoanController(
        ILoanService loanService,
        ILogger<LoanController> logger,
        IClientService clientService)
    {
        _loanService = loanService ?? throw new ArgumentNullException(nameof(loanService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
    }

    [HttpPost("apply-for-loan")]
    public async Task<ActionResult<LoanResponseDTO>> ApplyForLoan([FromBody] LoanDTO loanDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new ApiResponse(400, string.Join(", ", errors)));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized(new ApiResponse(401, "User is not authenticated."));
        }

        var client = await _clientService.GetClientByUserIdAsync(userId);
        if (client == null)
        {
            _logger.LogWarning("User not found for user ID: {UserId}", userId);
            return Unauthorized(new ApiResponse(401, "User not found or unauthorized"));
        }

        if (!client.IsEligibleForLoan())
        {
            return BadRequest(new ApiResponse(400, "Client is not eligible for a loan due to age restrictions"));
        }

        var loan = new Loan
        {
            Name = loanDto.Name,
            Amount = loanDto.Amount,
            RemainingAmount = loanDto.Amount,
            InterestRate = loanDto.InterestRate,
            DurationInMonths = loanDto.DurationInMonth,
            Status = LoanStatus.Pending,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(loanDto.DurationInMonth),
        };

        await _loanService.ApplyForLoan(client, loan);

        _logger.LogInformation("Loan application submitted successfully for client {ClientId}", client.Id);

        return Ok(new LoanResponseDTO
        {
            LoanId = loan.Id,
            Status = loan.Status,
            MonthlyPayment = loan.CalculateMonthlyPayment(),
            StartDate = loan.StartDate,
            EndDate = loan.EndDate
        });
    }

    [HttpGet("status/{id:guid}")]
    public async Task<ActionResult<LoanStatusDTO>> GetLoanStatus(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized(new ApiResponse(401, "User is not authenticated."));
        }

        var client = await _clientService.GetClientByUserIdWithLoansAsync(userId);
        if (client == null)
        {
            _logger.LogWarning("User not found for user ID: {UserId}", userId);
            return Unauthorized(new ApiResponse(401, "User not found or unauthorized"));
        }

        var loan = client.Loans.FirstOrDefault(l => l.Id == id);
        if (loan == null)
        {
            return NotFound(new ApiResponse(404, "Loan not found"));
        }

        var monthlyPayment = _loanService.CalculateConcreteLoanMonthlyPayment(client, loan);

        return Ok(new 
        {
            LoanId = loan.Id,
            Status = loan.Status.ToDisplayString(),
            MonthlyPayment = monthlyPayment,
            RemainingAmount = loan.RemainingAmount,
        });
    }
}

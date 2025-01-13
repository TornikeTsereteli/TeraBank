using System.Security.Claims;
using API.Response;
using Application.DTOs;
using Application.Mappers;
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

    /// <summary>
    /// Allows an authenticated client to apply for a new loan.
    /// </summary>
    /// <remarks>
    /// This endpoint validates the input loan details and checks if the authenticated client is eligible to apply for a loan.
    /// If all conditions are met, the loan application is submitted and a response is returned containing the details of the created loan.
    /// </remarks>
    /// <param name="loanDto">An object containing the details of the loan application, such as name, amount, interest rate, and duration.</param>
    /// <returns>
    /// An <see cref="ActionResult"/> containing:
    /// - 200 (OK): If the loan application is successfully submitted, returns a <see cref="LoanResponseDTO"/> with the loan details.
    /// - 400 (Bad Request): If the input data is invalid or the client is not eligible for a loan.
    /// - 401 (Unauthorized): If the user is not authenticated or the client is not found.
    /// </returns>
    /// <response code="200">The loan application was successfully submitted.</response>
    /// <response code="400">The input data is invalid or the client is ineligible for a loan.</response>
    /// <response code="401">The user is not authenticated or the client does not exist.</response>
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

        return Ok(Mapper.LoanToLoanResponseDto(loan));
    }

    /// <summary>
    /// Retrieves the status of a loan based on the provided loan ID.
    /// </summary>
    /// <remarks>
    /// This endpoint checks if the loan belongs to the client associated with the authenticated user. 
    /// If the loan exists and is associated with the client, the loan status is returned as a <see cref="LoanStatusDTO"/>.
    /// Otherwise, an appropriate response is returned.
    /// </remarks>
    /// <param name="id">The unique identifier (GUID) of the loan.</param>
    /// <returns>
    /// An <see cref="ActionResult"/> containing:
    /// - 200 (OK): If the loan status is successfully retrieved, returns a <see cref="LoanStatusDTO"/>.
    /// - 401 (Unauthorized): If the user is not authenticated
    /// </returns>
    /// <response code="200">The loan status was successfully retrieved.</response>
    /// <response code="401">Unauthorized access</response>
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
        
        return Ok(await _loanService.GetLoanStatus(client, id));
    }
}

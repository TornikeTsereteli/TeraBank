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
using Application.Mappers;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly ILoanService _loanService;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IClientService clientService, ILogger<ClientController> logger, ILoanService loanService)
        {
            _clientService = clientService;
            _logger = logger;
            _loanService = loanService;
        }

        [HttpGet("loan-history")]
        public async Task<IActionResult> GetClientLoanHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt - User ID is null or empty");
                return Unauthorized(new ApiResponse(401, "User is not authenticated."));
            }

            _logger.LogInformation("Fetching loan history for user ID: {UserId}", userId);

            var client = await _clientService.GetClientByUserIdWithLoansAsync(userId);
            if (client == null)
            {
                _logger.LogWarning("Client not found for user ID: {UserId}", userId);
                return NotFound(new ApiResponse(404, "Client not found."));
            }

            if (client.Loans == null || !client.Loans.Any())
            {
                _logger.LogInformation("No loan history found for user ID: {UserId}", userId);
                return Ok(new ApiResponse(200, "No loan history available.", new { loans = Enumerable.Empty<decimal>() }));
            }

            var loanHistory = client.Loans
                .Select(Mapper.LoanToLoanResponseDto)
                .ToList();

            _logger.LogInformation("Successfully retrieved loan history for user ID: {UserId}", userId);
            return Ok(new ApiResponse(200, "Loan history retrieved successfully.", new { loans = loanHistory }));
        }

        [HttpGet("penalty-fees")]
        public async Task<IActionResult> GetPenaltyFees()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt - User ID is null or empty");
                return Unauthorized();
            }

            _logger.LogInformation("Fetching penalty fees for user ID: {UserId}", userId);

            var client = await _clientService.GetClientByUserIdWithLoansAndPenaltiesAsync(userId);
            if (client == null)
            {
                _logger.LogWarning("Client not found for user ID: {UserId}", userId);
                return Unauthorized(401);
            }

            var fees = client.Loans.Select(loan => loan.Penalties).Aggregate(new List<Penalty>(), (acc, penalties) =>
            {
                acc.AddRange(penalties);
                return acc;
            }).Select(Mapper.PenaltyToPenaltyResponseDto);

            _logger.LogInformation("Successfully retrieved penalty fees for user ID: {UserId}", userId);
            return Ok(new ApiResponse(200, "Your penalty fees are", fees));
        }
    }
}

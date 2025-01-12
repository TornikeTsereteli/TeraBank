using System.Security.Claims;
using API.Response;
using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Services;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IClientService _clientService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<PaymentController> _logger; 

        public PaymentController(IPaymentService paymentService, UserManager<AppUser> userManager, IClientService clientService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _userManager = userManager;
            _clientService = clientService;
            _logger = logger;  
        }

        [HttpPost("make-penalty-payment")]
        public async Task<IActionResult> MakePenaltyPayment([FromBody] PenaltyPaymentDTO penaltyPaymentDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid parameters for penalty payment: {PenaltyPaymentDto}", penaltyPaymentDto);
                return BadRequest(new ApiResponse(400, "Invalid Parameters"));
            }

            var amount = penaltyPaymentDto.Amount;
            var penaltyId = penaltyPaymentDto.PenaltyID;

            _logger.LogInformation("Initiating penalty payment: Amount: {Amount}, Penalty ID: {PenaltyId}", amount, penaltyId);

            await _paymentService.MakePenaltyPayment(amount, penaltyId);

            _logger.LogInformation("Penalty payment successfully completed: Amount: {Amount}, Penalty ID: {PenaltyId}", amount, penaltyId);
            return Ok("Penalty SuccessFully Paid");
        }

        [HttpPost("make-loan-payment")]
        public async Task<IActionResult> MakeLoanPayment([FromBody] PaymentDTO paymentDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid parameters for loan payment: {PaymentDto}", paymentDto);
                return BadRequest(new ApiResponse(400, "Invalid Parameters"));
            }

            var amount = paymentDto.Amount;
            var id = paymentDto.LoanId;

            _logger.LogInformation("Initiating loan payment: Amount: {Amount}, Loan ID: {LoanId}", amount, id);

            await _paymentService.MakePayment(amount, id);

            _logger.LogInformation("Loan payment successfully completed: Amount: {Amount}, Loan ID: {LoanId}", amount, id);
            return Ok("Successfully Paid");
        }

        [HttpGet("payment-history")]
        public async Task<IActionResult> GetPaymentHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt - User ID is null or empty");
                return Unauthorized();
            }

            _logger.LogInformation("Fetching payment history for user ID: {UserId}", userId);

            var client = await _clientService.GetClientByUserIdWithLoansAndPaymentsAsync(userId);

            if (client == null)
            {
                _logger.LogWarning("Client not found for user ID: {UserId}", userId);
                return Unauthorized();
            }

            var result = await _paymentService.GetAllPayments(client);

            _logger.LogInformation("Successfully fetched payment history for user ID: {UserId}", userId);
            return Ok(result.Select(x => new
            {
                x.Id,
                x.Amount,
                x.PaymentDate,
            }));
        }
    }
}

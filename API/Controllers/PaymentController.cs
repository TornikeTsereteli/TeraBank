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

namespace API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{

    private readonly IPaymentService _paymentService;
    private readonly IClientService _clientService;
    private readonly UserManager<AppUser> _userManager;
    
    
    public PaymentController(IPaymentService paymentService, UserManager<AppUser> userManager, IClientService clientService)
    {
        _paymentService = paymentService;
        _userManager = userManager;
        _clientService = clientService;
    }


    [HttpPost("make-penalty-payment")]
    public async Task<IActionResult> MakePenaltyPayment([FromBody] PenaltyPaymentDTO penaltyPaymentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse(400, "Invalid Parameters"));
        }

        var amount = penaltyPaymentDto.Amount;
        var penaltyId = penaltyPaymentDto.PenaltyID;

        await _paymentService.MakePenaltyPayment(amount, penaltyId);

        return Ok("Penalty SuccessFully Paid");
    }
    
    

    // this function does not let you pay more than monthly payment in a month
    [HttpPost("make-loan-payment")]
    
    public async Task<IActionResult> MakeLoanPayment([FromBody] PaymentDTO paymentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse(400, "Invalid Parameters"));
        }
        var amount = paymentDto.Amount;
        var id = paymentDto.LoanId;
            
        await _paymentService.MakePayment(amount, id);

        
        return Ok("Successfully Paid");
    }  
   
    
    
    

    [HttpGet("payment-history")]
    public async Task<IActionResult> GetPaymentHistory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var client = await _clientService.GetClientByUserIdWithLoansAndPaymentsAsync(userId);

        if (client == null)
        {
            return Unauthorized();
        }
        
        var result = await _paymentService.GetAllPayments(client);
        return Ok(result.Select(x =>new 
        {
            x.Id,
            x.Amount,
            x.PaymentDate,
        }));
    }
    

}
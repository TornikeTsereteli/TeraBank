using Application.DTOs;
using Domain.Interfaces;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{

    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }
    

    // this function does not let you pay more than monthly payment in a month
    [HttpPost("pay")]
    public async Task<IActionResult> MakePayment([FromBody] PaymentDTO paymentDto)
    {
        var amount = paymentDto.Amount;
        var id = paymentDto.LoanId;
        await _paymentService.MakePayment(amount, id);
        return Ok("Successfully Paid");
    }  
   
    
    
    

    [HttpGet("history")]
    public async Task<IActionResult> GetPaymentHistory()
    {
        return null;
    }
    
}
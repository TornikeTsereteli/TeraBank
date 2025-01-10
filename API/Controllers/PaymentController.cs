using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    [HttpPost("pay")]
    public async Task<IActionResult> MakePayment()
    {
        return Ok();
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetPaymentHistory()
    {
        return Ok();
    }
    
}
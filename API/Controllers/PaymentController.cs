using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    public async Task<IActionResult> MakePayment()
    {
        return Ok();
    }

    public async Task<IActionResult> GetPaymentHistory()
    {
        return Ok();
    }
    
}
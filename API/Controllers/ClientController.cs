using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientController : ControllerBase
{
    [HttpGet("details")]
    public async Task<IActionResult> GetClientDetails()
    {
        return Ok();
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetClientLoanHistory()
    {
        return Ok();
    }
    
}
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoanController : ControllerBase
{
    [HttpPost("apply")]
    public async Task<IActionResult> ApplyForLoan([FromBody] LoanDTO loanDto)
    {
        return Ok();
    }

    public async Task<IActionResult> GetLoanStatus()
    {
        return Ok("dada");
    } 
}
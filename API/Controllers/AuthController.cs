using System.Security.Claims;
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

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IEmailSender<AppUser> _emailSender;
    private readonly IClientService _clientService;
    
    public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,IEmailSender<AppUser> emailSender, IClientService clientService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _clientService = clientService;
    }
    
    [HttpPost("/register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Client client = new Client()
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            CreditScore = 600,
            DateOfBirth = registerDto.DateOfBirth
        };
        AppUser user = new AppUser()
        {
            UserName = "terra"+registerDto.FirstName,
            Email = registerDto.Email,
            Client = client
        };
        await _clientService.AddClientAsync(client);
        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (result.Succeeded)
        {

            await _userManager.AddToRoleAsync(user, "User");
            
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmationLink = Url.Action(
                "ConfirmEmail", 
                "Auth", 
                new { userId = user.Id, token }, 
                Request.Scheme);

            await _emailSender.SendConfirmationLinkAsync(user, user.Email, confirmationLink);
            
            
            return Ok(new
            {
                Message = "Registration successful, please check yor email"
            });
        }

        return BadRequest(result.Errors);
    }
      [HttpGet("/confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            return BadRequest("Invalid email confirmation request.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound("User not found.");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            return Ok(new { Message = "Email confirmed successfully!" });
        }

        return BadRequest("Email confirmation failed.");
    }


    [HttpPost("/send-verification-notification")]
    public async Task<IActionResult> SendVerificationNotification([FromBody] string email)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest(new { Message = "Email is required." });

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound(new { Message = "User not found." });

        if (await _userManager.IsEmailConfirmedAsync(user))
            return BadRequest(new { Message = "Email is already confirmed." });

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = Url.Action(
            "ConfirmEmail",
            "Auth",
            new { userId = user.Id, token },
            Request.Scheme);
        await _emailSender.SendConfirmationLinkAsync(user, user.Email, confirmationLink);

        return Ok(new { Message = "Verification email sent successfully." });
    }
    

    [HttpPost("/login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(loginDto.Email);

        if (user == null)
        {
            Console.WriteLine("Im here");
            return Unauthorized("Invalid Email or Password");
        }
        
        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            Console.WriteLine("emaill >!=?!??????");
            return Unauthorized("Please confirm your email before logging in.");
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName, loginDto.Password, false, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return Ok("Login Successful");
        }
        
        if (result.IsLockedOut)
        {
            return Unauthorized("Your account is locked due to too many failed login attempts.");
        }

        return Unauthorized("Invalid email or password.");
    }

    [HttpPost("/logout")]
    public async Task<IActionResult> LogOut()
    {
        await _signInManager.SignOutAsync();
        return Ok(new
        {
            Message = "Successfully Logged Out"
        });    
    }

    [Authorize(Roles = "User")]
    [HttpGet("/user")]
    public async Task<IActionResult> UserPage()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        var userName = User.Identity?.Name;
        bool isEmailConfirmed = false;

        var user = await _userManager.Users
            .Include(u => u.Client)
            .FirstOrDefaultAsync(u => u.Id == userId);
        isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        return Ok(new
            {
                Id = userId,
                Email = userEmail,
                Username = userName,
                IsVerified = isEmailConfirmed,
                Message = "Welcome to your page!",
                Client = user?.Client
            });
    }
    

    [Authorize(Roles = "Admin")]
    [HttpGet("/admin")]
    public async Task<IActionResult> AdminPage()
    {
        return Ok("admin page");
    }
    
}
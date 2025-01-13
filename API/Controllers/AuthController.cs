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
using Microsoft.Extensions.Logging;

// Basic Controller Class , Stadart methtod, register, Login, SignOut ...., after user is register he needs email verification, mail is automatically send to him


namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender<AppUser> _emailSender;
        private readonly IClientService _clientService;
        private readonly IUnitOfWork _unitOfWork; // maybe can be used ti run something using transaction
        private readonly ILogger<AuthController> _logger;
        private readonly ICreditPointStrategy _creditPointStrategy; // credit point strategy, I have written just moq class which returns 600

        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailSender<AppUser> emailSender, IClientService clientService, ILogger<AuthController> logger, ICreditPointStrategy creditPointStrategy)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _clientService = clientService;
            _logger = logger;
            _creditPointStrategy = creditPointStrategy;
        }

        [HttpPost("/register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Registration failed: Invalid model state.");
                return BadRequest(ModelState);
            }

            Client client = new Client()
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                CreditScore = _creditPointStrategy.GetCreditPoint(),
                DateOfBirth = registerDto.DateOfBirth
            };
            AppUser user = new AppUser()
            {
                UserName = "terra" + registerDto.FirstName,
                Email = registerDto.Email,
                Client = client
            };

            _logger.LogInformation("Attempting to register user: {Email}", registerDto.Email);
            await _clientService.AddClientAsync(client);
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "Auth", new { userId = user.Id, token }, Request.Scheme);
                await _emailSender.SendConfirmationLinkAsync(user, user.Email, confirmationLink);

                _logger.LogInformation("Registration successful for user: {Email}", registerDto.Email);
                return Ok(new { Message = "Registration successful, please check your email" });
            }

            _logger.LogError("Registration failed for user: {Email} - Errors: {Errors}", registerDto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest(result.Errors);
        }

        [HttpGet("/confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Email confirmation failed: Invalid parameters.");
                return BadRequest("Invalid email confirmation request.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Email confirmation failed: User not found, UserId = {UserId}", userId);
                return NotFound("User not found.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                _logger.LogInformation("Email confirmed successfully for user: {UserId}", userId);
                return Ok(new { Message = "Email confirmed successfully!" });
            }

            _logger.LogError("Email confirmation failed for user: {UserId}", userId);
            return BadRequest("Email confirmation failed.");
        }

        [HttpPost("/send-verification-notification")]
        public async Task<IActionResult> SendVerificationNotification([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Send verification notification failed: Email is required.");
                return BadRequest(new { Message = "Email is required." });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Send verification notification failed: User not found, Email = {Email}", email);
                return NotFound(new { Message = "User not found." });
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                _logger.LogWarning("Send verification notification failed: Email already confirmed, Email = {Email}", email);
                return BadRequest(new { Message = "Email is already confirmed." });
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Auth", new { userId = user.Id, token }, Request.Scheme);
            await _emailSender.SendConfirmationLinkAsync(user, user.Email, confirmationLink);

            _logger.LogInformation("Verification email sent successfully to: {Email}", email);
            return Ok(new { Message = "Verification email sent successfully." });
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login failed: Invalid model state.");
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: Invalid email or password, Email = {Email}", loginDto.Email);
                return Unauthorized("Invalid Email or Password");
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                _logger.LogWarning("Login failed: Email not confirmed, Email = {Email}", loginDto.Email);
                return Unauthorized("Please confirm your email before logging in.");
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, loginDto.Password, false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("Login successful for user: {Email}", loginDto.Email);
                return Ok("Login Successful");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Login failed: Account is locked out, Email = {Email}", loginDto.Email);
                return Unauthorized("Your account is locked due to too many failed login attempts.");
            }

            _logger.LogWarning("Login failed: Invalid email or password, Email = {Email}", loginDto.Email);
            return Unauthorized("Invalid email or password.");
        }

        [HttpPost("/logout")]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out successfully.");
            return Ok(new { Message = "Successfully Logged Out" });
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

            _logger.LogInformation("User accessed their page: {Email}", userEmail);

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
            _logger.LogInformation("Admin accessed the admin page.");
            return Ok("admin page");
        }
    }
}

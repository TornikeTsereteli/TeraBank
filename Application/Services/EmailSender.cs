using System.Net;
using System.Net.Mail;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class EmailSender<TUser> : IEmailSender<TUser> where TUser: IdentityUser
{

    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendConfirmationLinkAsync(TUser user, string email, string confirmationLink)
    {
        await SendEmailAsync(email, "Confirm Your Email",
            $"Please confirm your email by clicking this link: <a href='{confirmationLink}'>Confirm Email</a>");
    }

    public async Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink)
    {
        await SendEmailAsync(email, "Reset Your Password",
            $"You can reset your password by clicking this link: <a href='{resetLink}'>Reset Password</a>");
    }

    public async Task SendPasswordResetCodeAsync(TUser user, string email, string resetCode)
    {
        
        await SendEmailAsync(email, "Reset Your Password",
            $"Use the following code to reset your password: {resetCode}");
    }

    private async Task SendEmailAsync(string email, string subject, string message)
    {
        var smtpClient = new SmtpClient
        {
            Host = _configuration["EmailSettings:Host"],
            Port = int.Parse(_configuration["EmailSettings:Port"]),
            Credentials = new NetworkCredential(
                _configuration["EmailSettings:Username"],
                _configuration["EmailSettings:Password"]),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["EmailSettings:FromEmail"]),
            Subject = subject,
            Body = message,
            IsBodyHtml = true
        };
        
        mailMessage.Headers.Add("Content-Type","text/html");
        mailMessage.To.Add(email);

        await smtpClient.SendMailAsync(mailMessage);
    }

    
}
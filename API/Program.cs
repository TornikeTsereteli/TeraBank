using System.Collections.Immutable;
using API.EmailSender;
using API.Middlewares;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Services;
using Infrastructure.Database;
using Infrastructure.Identity;
using Infrastructure.Repositories;
using Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Logging.AddConsole();  
builder.Logging.AddDebug();
builder.Logging.AddEventLog(); 


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();



var databasePath = Path.Combine(AppContext.BaseDirectory, "..","..", "..", "..", "Infrastructure", "Database", "db.sqlite");
var connectionString = $"Data Source={Path.GetFullPath(databasePath)}";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Register database context and identity 
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
// {
//     options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")); 
// });

builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Register repositories
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPenaltyRepository, PenaltyRepository>();
builder.Services.AddScoped<IPaymentScheduleRepository, PaymentScheduleRepository>();

// Register services
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Register unit of work, I use it just for Transaction
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// register strategies this classes are just moq classes one o
builder.Services.AddSingleton<ICreditPointStrategy, CreditPointStrategy>();
builder.Services.AddSingleton<ILoanApproveStrategy, LoanApproveStrategy>();
builder.Services.AddScoped<IMoneySentBackStrategy, MoneySentBackStrategy>();

// Register email sender service
builder.Services.AddScoped<IEmailSender<AppUser>, EmailSender<AppUser>>();

// Register background services
builder.Services.AddSingleton<IHostedService, PaymentCheckService>(); // this background task is responsible to check every day in once if someone have unpaid fee, or did not pay last months loan payment => gamowere axali jarima!!!

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Seed roles in the database
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    await RoleSeeder.SeedAsync(serviceProvider);
}

// Global exception handling middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

// Map controllers to endpoints
app.MapControllers();

// Run the application
app.Run();

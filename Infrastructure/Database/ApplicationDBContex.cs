using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    
    public DbSet<Client> Clients { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<Payment> Payments { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>().HasOne(user => user.Client)
            .WithOne()
            .HasForeignKey<AppUser>(user => user.ClientId);

        builder.Entity<Client>()
            .HasKey(c => c.Id);

        builder.Entity<Client>()
            .HasMany(c => c.Loans)
            .WithOne(l => l.Client)
            .HasForeignKey(l => l.ClientId);

        builder.Entity<Loan>()
            .HasKey(l => l.Id);
        
        builder.Entity<Loan>()
            .HasMany(l => l.Payments)
            .WithOne(p => p.Loan)
            .HasForeignKey(p => p.LoanId);

        builder.Entity<Payment>()
            .HasKey(p => p.Id);
    }
}
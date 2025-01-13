using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Database;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    
    public DbSet<Client> Clients { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Penalty> Penalties { get; set; }
    public DbSet<PaymentSchedule> PaymentSchedules { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure AppUser and Client relationship
        builder.Entity<AppUser>()
            .HasOne(user => user.Client)
            .WithOne()
            .HasForeignKey<AppUser>(user => user.ClientId);

        // Configure Client relationships
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

        builder.Entity<Loan>()
            .HasMany(l => l.Penalties) 
            .WithOne(p => p.Loan)
            .HasForeignKey(p => p.LoanId);
        
        builder.Entity<Loan>()
            .HasMany(l => l.PaymentSchedules) 
            .WithOne(p => p.Loan)
            .HasForeignKey(p => p.LoanId);
        
        // Configure Payment
        builder.Entity<Payment>()
            .HasKey(p => p.Id);

        // Configure Penalty
        builder.Entity<Penalty>()
            .HasKey(p => p.Id);

        builder.Entity<Penalty>()
            .Property(p => p.Amount)
            .HasColumnType("decimal(18,2)") 
            .IsRequired();

        builder.Entity<Penalty>()
            .Property(p => p.Reason)
            .HasMaxLength(255) 
            .IsRequired();

        builder.Entity<Penalty>()
            .Property(p => p.IsPaid)
            .HasDefaultValue(false);

        builder.Entity<Penalty>()
            .Property(p => p.ImposedDate)
            .IsRequired();

        builder.Entity<Penalty>()
            .HasOne(p => p.Loan) 
            .WithMany(l => l.Penalties)
            .HasForeignKey(p => p.LoanId);
        
        builder.Entity<Loan>(entity =>
        {
            entity.Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<LoanStatus>()); // Store LoanStatus as a string
        });

        builder.Entity<PaymentSchedule>()
            .HasKey(p => p.Id);
    }

}
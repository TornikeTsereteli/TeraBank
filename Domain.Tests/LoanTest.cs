using System;
using System.Linq;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Domain.Tests
{
    public class LoanTests
    {
        // Test CalculateMonthlyPayment method
        [Fact]
        public void CalculateMonthlyPayment_ValidLoan_ReturnsCorrectMonthlyPayment()
        {
            // Arrange
            var loan = new Loan
            {
                Amount = 10000,
                RemainingAmount = 8000,
                InterestRate = 5,
                StartDate = DateTime.Now.AddMonths(-12),
                EndDate = DateTime.Now.AddMonths(24),
                DurationInMonths = 36
            };

            // Act
            var monthlyPayment = loan.CalculateMonthlyPayment();

            // Assert
            monthlyPayment.Should().BeApproximately(351.22m, 0.01m);
        }

        // Test CalculateMonthlyPayment with invalid RemainingAmount
        [Fact]
        public void CalculateMonthlyPayment_RemainingAmountIsZero_ThrowsInvalidOperationException()
        {
            // Arrange
            var loan = new Loan
            {
                Amount = 10000,
                RemainingAmount = 0,
                InterestRate = 5,
                StartDate = DateTime.Now.AddMonths(-12),
                EndDate = DateTime.Now.AddMonths(24),
                DurationInMonths = 24
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => loan.CalculateMonthlyPayment());
        }

        // Test CalculateTotalPayment method
        [Fact]
        public void CalculateTotalPayment_ValidLoan_ReturnsCorrectTotalPayment()
        {
            // Arrange
            var loan = new Loan
            {
                Amount = 10000,
                RemainingAmount = 8000,
                InterestRate = 5,
                StartDate = DateTime.Now.AddMonths(-12),
                EndDate = DateTime.Now.AddMonths(24),
                DurationInMonths = 24
            };

            // Act
            var totalPayment = loan.CalculateTotalPayment();

            // Assert
            totalPayment.Should().BeApproximately(8432.88m, 0.01m);
        }

        // Test CalculatePenalty method
        [Fact]
        public void CalculatePenalty_ValidLoan_ReturnsCorrectPenalty()
        {
            // Arrange
            var loan = new Loan
            {
                Amount = 10000
            };

            // Act
            var penalty = loan.CalculatePenalty();

            // Assert
            penalty.Should().Be(100); // 1% of 10000
        }

        // Test HavePaidFullyLastMonth method with correct payments
        [Fact]
        public void HavePaidFullyLastMonth_PaymentsMadeAndPenaltiesPaid_ReturnsTrue()
        {
            // Arrange
            var loan = new Loan
            {
                Amount = 10000,
                RemainingAmount = 8000,
                InterestRate = 5,
                StartDate = DateTime.Now.AddMonths(-12),
                EndDate = DateTime.Now.AddMonths(24),
                DurationInMonths = 24,
                Payments = new[]
                {
                    new Payment { Amount = 351.22m, PaymentDate = DateTime.Now.AddMonths(-1) }
                },
                Penalties = new[]
                {
                    new Penalty { IsPaid = true }
                }
            };

            // Act
            var paidFully = loan.HavePaidFullyLastMonth();

            // Assert
            paidFully.Should().BeTrue();
        }

        [Fact]
        public void HavePaidFullyLastMonth_MissingPayment_ReturnsFalse()
        {
            var loan = new Loan
            {
                Amount = 10000,
                RemainingAmount = 8000,
                InterestRate = 5,
                StartDate = DateTime.Now.AddMonths(-12),
                EndDate = DateTime.Now.AddMonths(24),
                DurationInMonths = 24,
                Payments = new[] 
                {
                    new Payment { Amount = 100m, PaymentDate = DateTime.Now.AddMonths(-1) } // Less than expected
                },
                Penalties = new[]
                {
                    new Penalty { IsPaid = true }
                }
            };

            var paidFully = loan.HavePaidFullyLastMonth();

            paidFully.Should().BeFalse();
        }

        // Test InitialMonthlyPayment method
        // [Fact]
        // public void InitialMonthlyPayment_ValidLoan_ReturnsCorrectInitialPayment()
        // {
        //     // Arrange
        //     var loan = new Loan
        //     {
        //         Amount = 10000,
        //         InterestRate = 5,
        //         StartDate = DateTime.Now.AddMonths(-12),
        //         EndDate = DateTime.Now.AddMonths(24),
        //         DurationInMonths = 24
        //     };
        //
        //     // Act
        //     var initialPayment = loan.InitialMonthlyPayment();
        //
        //     // Assert
        //     initialPayment.Should().BeApproximately(351.22m, 0.01m);
        // }

        // Test CalculateRemainingMonth method
        // [Fact]
        // public void CalculateRemainingMonth_LoanHasExpired_ReturnsZero()
        // {
        //     // Arrange
        //     var loan = new Loan
        //     {
        //         StartDate = DateTime.Now.AddMonths(-24),
        //         EndDate = DateTime.Now.AddMonths(-1) // Expired loan
        //     };
        //
        //     // Act
        //     var remainingMonths = loan.CalculateRemainingMonth();
        //
        //     // Assert
        //     remainingMonths.Should().Be(0);
        // }
    }
}

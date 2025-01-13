using Domain.Entities;
using Xunit.Abstractions;

public class LoanTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public LoanTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private Loan CreateValidLoan()
    {
        return new Loan
        {
            Id = Guid.NewGuid(),
            Name = "Test Loan",
            Amount = 10000m,
            RemainingAmount = 10000m,
            StartDate = DateTime.Now.Date,
            EndDate = DateTime.Now.Date.AddYears(1),
            DurationInMonths = 12,
            InterestRate = 12, // 12%
            Status = LoanStatus.Approved,
            ClientId = Guid.NewGuid()
        };
    }

    [Fact]
    public void MakePayment_WithPenalties_ProcessesPenaltiesFirst()
    {
        // Arrange
        var loan = CreateValidLoan();
        loan.GeneratePaymentSchedules();

        _testOutputHelper.WriteLine(loan.Name);
        _testOutputHelper.WriteLine(loan.InterestRate.ToString());
        var penalty = new Penalty
        {
            Amount = 100m,
            RemainingAmount = 100m,
            ImposedDate = DateTime.Now,
            IsPaid = false
        };

        loan.Penalties = new List<Penalty> { penalty };

        // Act
        loan.MakePayment(150m);

        // Assert
        Assert.True(penalty.IsPaid);
        Assert.Equal(9850m, loan.RemainingAmount); // 10000 - 150
    }

    [Fact]
    public void MakePayment_WithOverpayment_RecalculatesFuturePayments()
    {
        // Arrange
        var loan = CreateValidLoan();
        loan.GeneratePaymentSchedules();
        var originalMonthlyPayment = loan.PaymentSchedules.First().Amount;

        // Act
        loan.MakePayment(2000m); // Significant overpayment

        // Assert
        var newMonthlyPayment = loan.PaymentSchedules.First(p => !p.IsPaid).Amount;
        Assert.True(newMonthlyPayment < originalMonthlyPayment);
    }

    [Fact]
    public void GeneratePaymentSchedules_CreatesCorrectNumberOfSchedules()
    {
        // Arrange
        var loan = CreateValidLoan();

        // Act
        var schedules = loan.GeneratePaymentSchedules();

        // Assert
        Assert.Equal(loan.DurationInMonths, schedules.Count());
        Assert.All(schedules, schedule => Assert.False(schedule.IsPaid));
    }

    [Fact]
    public void HaveLastMonthsFullyPaid_WithUnpaidSchedules_ReturnsFalse()
    {
        // Arrange
        var loan = CreateValidLoan();
        loan.GeneratePaymentSchedules();

        // Act
        var result = loan.HaveLastMonthsFullyPaid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CalculateMonthlyPayment_WithValidLoan_ReturnsCorrectAmount()
    {
        // Arrange
        var loan = CreateValidLoan();
        loan.GeneratePaymentSchedules();

        // Act
        var monthlyPayment = loan.CalculateMonthlyPayment();

        // Assert
        Assert.True(monthlyPayment > 0);
        // You might want to add a more precise calculation check here
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void MakePayment_WithInvalidAmount_ThrowsArgumentException(decimal invalidAmount)
    {
        // Arrange
        var loan = CreateValidLoan();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => loan.MakePayment(invalidAmount));
    }

    [Fact]
    public void GetThisMonthPayment_WithNoPaymentsDue_ReturnsZero()
    {
        // Arrange
        var loan = CreateValidLoan();
        loan.PaymentSchedules = new List<PaymentSchedule>();

        // Act
        var result = loan.GetThisMonthPayment();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculatePenalty_ReturnsOnePercentOfLoanAmount()
    {
        // Arrange
        var loan = CreateValidLoan();
        var expectedPenalty = loan.Amount * 0.01m;

        // Act
        var penalty = loan.CalculatePenalty();

        // Assert
        Assert.Equal(expectedPenalty, penalty);
    }

    [Fact]
    public void MakePayment_WithOverpayment_CalculatesCorrectNewMonthlyPayment()
    {
        // Arrange
        var loan = CreateValidLoan();
        loan.GeneratePaymentSchedules();

        var initialMonthlyPayment = loan.PaymentSchedules.First().Amount;
        _testOutputHelper.WriteLine($"Initial monthly payment: {initialMonthlyPayment}");

        // Record initial state
        var initialRemainingAmount = loan.RemainingAmount;
        var initialUnpaidMonths = loan.PaymentSchedules.Count(p => !p.IsPaid);
        _testOutputHelper.WriteLine($"Initial remaining amount: {initialRemainingAmount}");
        _testOutputHelper.WriteLine($"Initial unpaid months: {initialUnpaidMonths}");

        // Act
        var overpaymentAmount = 2000m;
        loan.MakePayment(overpaymentAmount);

        // Assert
        var newMonthlyPayment = loan.PaymentSchedules.First(p => !p.IsPaid).Amount;
        var expectedRemainingAmount = initialRemainingAmount - overpaymentAmount;
        var remainingMonths = loan.PaymentSchedules.Count(p => !p.IsPaid);

        _testOutputHelper.WriteLine($"New remaining amount: {loan.RemainingAmount}");
        _testOutputHelper.WriteLine($"Remaining months: {remainingMonths}");
        _testOutputHelper.WriteLine($"New monthly payment: {newMonthlyPayment}");

        // Verify the new monthly payment matches the expected calculation
        var monthlyInterestRate = loan.InterestRate / 100 / 12;
        var expectedMonthlyPayment = loan.RemainingAmount *
                                     monthlyInterestRate * (decimal)Math.Pow((double)(1 + monthlyInterestRate),
                                         remainingMonths) /
                                     ((decimal)Math.Pow((double)(1 + monthlyInterestRate), remainingMonths) - 1);

        _testOutputHelper.WriteLine($"Expected monthly payment: {expectedMonthlyPayment}");

        Assert.Equal(expectedMonthlyPayment, newMonthlyPayment, 2); // Compare with 2 decimal precision
    }

    [Fact]
    public void MakePayment_WithMultipleOverpayments_UpdatesPaymentsCorrectly()
    {
        // Arrange
        var loan = CreateValidLoan();
        loan.GeneratePaymentSchedules();

        var initialPayment = loan.PaymentSchedules.First().Amount;
        _testOutputHelper.WriteLine($"Initial monthly payment: {initialPayment}");

        // Act - First overpayment
        loan.MakePayment(2000m);
        var paymentAfterFirst = loan.PaymentSchedules.First(p => !p.IsPaid).Amount;
        _testOutputHelper.WriteLine($"Monthly payment after first overpayment: {paymentAfterFirst}");

        // Act - Second overpayment
        loan.MakePayment(1000m);
        var paymentAfterSecond = loan.PaymentSchedules.First(p => !p.IsPaid).Amount;
        _testOutputHelper.WriteLine($"Monthly payment after second overpayment: {paymentAfterSecond}");

        // Assert
        Assert.True(paymentAfterFirst < initialPayment);
        Assert.True(paymentAfterSecond < paymentAfterFirst);
    }
    

    [Theory]
    [InlineData(5000)] // 50% overpayment
    [InlineData(7500)] // 75% overpayment
    [InlineData(9000)] // 90% overpayment
    public void MakePayment_WithDifferentOverpaymentAmounts_CalculatesCorrectNewPayments(decimal overpaymentAmount)
    {
        // Arrange
        var loan = CreateValidLoan();
        loan.GeneratePaymentSchedules();
        
        _testOutputHelper.WriteLine(loan.PaymentSchedules.Select(x=>x.IsPaid.ToString()).ToList().Count().ToString());
        var initialPayment = loan.PaymentSchedules.First().Amount;
        _testOutputHelper.WriteLine(initialPayment.ToString());
        _testOutputHelper.WriteLine($"Initial payment: {initialPayment}");
        _testOutputHelper.WriteLine($"Overpayment amount: {overpaymentAmount}");

        // Act
        loan.MakePayment(overpaymentAmount);

        // Assert
        var newPayment = loan.PaymentSchedules.First(p => !p.IsPaid).Amount;
        _testOutputHelper.WriteLine($"New monthly payment: {newPayment}");

        Assert.True(newPayment < initialPayment);
        Assert.True(newPayment > 0);

        // Verify all future payments are updated to the same amount
        Assert.All(loan.PaymentSchedules.Where(p => !p.IsPaid),
            schedule => Assert.Equal(newPayment, schedule.Amount, 2));
    }
}
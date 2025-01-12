using System.Collections;

namespace Domain.Entities
{
    public class Loan
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public decimal Amount { get; set; }
        public decimal RemainingAmount { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int DurationInMonths { get; set; }

        public decimal InterestRate { get; set; }
        public LoanStatus Status { get; set; } = LoanStatus.Pending;

        public Guid ClientId { get; set; }
        public Client Client { get; set; }

        public IEnumerable<Payment> Payments { get; set; } = new List<Payment>();

        public IEnumerable<Penalty> Penalties { get; set; } = new List<Penalty>();

        public decimal CalculateMonthlyPayment()
        {
            if (RemainingAmount <= 0) throw new InvalidOperationException("Remaining amount must be greater than 0.");

            var monthlyRate = InterestRate / 100 / 12;
            int remainingMonths = CalculateRemainingMonth();

            if (remainingMonths <= 0) throw new InvalidOperationException("Remaining months must be greater than 0.");

            return RemainingAmount * monthlyRate / (1 - (decimal)Math.Pow(1 + (double)monthlyRate, -remainingMonths));
        }

        
        private int CalculateRemainingMonth()
        {
            var now = DateTime.Now;
            if (now > EndDate) return 0;  // If current date is past the loan end date, return 0
            var months = ((EndDate.Year - now.Year) * 12) + EndDate.Month - now.Month;
            return months;
        }

        public decimal CalculateTotalPayment()
        {
            var monthlyPayment = CalculateMonthlyPayment();
            int remainingMonths = CalculateRemainingMonth();

            return monthlyPayment * remainingMonths;
        }

        public decimal CalculatePenalty()
        {
            return Amount * (decimal)0.01;
        }
        
        
        
        public bool HavePaidFullyLastMonth()
        {
            var now = DateTime.Now;
            var paymentDay = StartDate.Day;
            
            DateTime lastPaymentPeriodStart;
            DateTime lastPaymentPeriodEnd;

            if (now.Day < paymentDay)
            {
                lastPaymentPeriodEnd = new DateTime(now.Year, now.Month, paymentDay).AddDays(-1);
                lastPaymentPeriodStart = lastPaymentPeriodEnd.AddMonths(-1).AddDays(1);
            }
            else
            {
                lastPaymentPeriodStart = new DateTime(now.Year, now.Month, paymentDay);
                lastPaymentPeriodEnd = lastPaymentPeriodStart.AddMonths(1).AddDays(-1);
            }

            decimal expectedPayment = InitialMonthlyPayment();

            decimal paymentsMade = Payments
                .Where(p => p.PaymentDate >= lastPaymentPeriodStart && p.PaymentDate <= lastPaymentPeriodEnd)
                .Sum(p => p.Amount);

            bool penaltiesPaid = Penalties.All(p => p.IsPaid);

            return paymentsMade >= expectedPayment && penaltiesPaid;
        }
        
        private decimal InitialMonthlyPayment()
        {
            if (Amount <= 0) throw new InvalidOperationException("Remaining amount must be greater than 0.");

            var monthlyRate = InterestRate / 100 / 12;
            int remainingMonths = CalculateRemainingMonth();

            if (remainingMonths <= 0) throw new InvalidOperationException("Remaining months must be greater than 0.");

            return Amount * monthlyRate / (1 - (decimal)Math.Pow(1 + (double)monthlyRate, -remainingMonths));
        }
    }

    public enum LoanStatus
    {
        Pending,
        Approved,
        Rejected,
        Completed
        
        
    }
}

namespace Domain.Entities
{
    public class Loan
    {
        public Guid Id { get; set; }

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
            decimal penalty = 0;

            if (Status == LoanStatus.Pending && DateTime.Now > EndDate)
            {
                var overdueMonths = (DateTime.Now.Year - EndDate.Year) * 12 + DateTime.Now.Month - EndDate.Month;
                if (overdueMonths > 0)
                {
                    penalty = RemainingAmount * 0.01m * overdueMonths;
                }
            }

            return penalty;
        }

        public void HandleOverpayment(decimal overpaymentAmount)
        {
            if (overpaymentAmount <= 0) throw new InvalidOperationException("Overpayment amount must be greater than zero.");

            RemainingAmount -= overpaymentAmount;

            if (RemainingAmount <= 0)
            {
                Status = LoanStatus.Completed;
                RemainingAmount = 0;  // Ensure no negative remaining balance
            }
        }

        public void MarkAsCompleted()
        {
            if (RemainingAmount <= 0)
            {
                Status = LoanStatus.Completed;
            }
            else
            {
                throw new InvalidOperationException("Loan cannot be marked as completed with remaining balance.");
            }
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

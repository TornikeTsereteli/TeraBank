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

        public List<PaymentSchedule> PaymentSchedules { get; set; } = [];

        public IEnumerable<Payment> Payments { get; set; } = new List<Payment>();

        public IEnumerable<Penalty> Penalties { get; set; } = new List<Penalty>();
        
        // public Loan()
        // {
        //     if (InterestRate <= 0)
        //         throw new ArgumentException("Interest rate must be positive");
        //     if (Amount <= 0)
        //         throw new ArgumentException("Loan amount must be positive");
        //     if (StartDate >= EndDate)
        //         throw new ArgumentException("Start date must be before end date");
        //     
        //     if (Status != LoanStatus.Approved)
        //         throw new InvalidOperationException("Cannot make payment on a non-approved loan");
        // }
        
        private const decimal Tolerance = 0.01m;
        
        public bool HaveLastMonthsFullyPaid()
        {
            DateTime lastMonthPaymentDateTime = GetLastMonthPaymentDay(StartDate.Day);
            var paymentSchedules = PaymentSchedules
                .Where(x => x.PaymentDay <= lastMonthPaymentDateTime)
                .ToList(); 

            return paymentSchedules.All(x => x.IsPaid) && Penalties.All(x=>x.IsPaid);
        }

        public decimal CalculateMonthlyPayment()
        {
            decimal monthlyInterestRate = InterestRate / 100 / 12;
            int remainingMonths = PaymentSchedules.Count(p => !p.IsPaid);

            if (remainingMonths == 0)
            {
                return 0;
            }

            decimal monthlyPayment = RemainingAmount * (monthlyInterestRate * (decimal)Math.Pow((double)(1 + monthlyInterestRate), remainingMonths)) /
                                     ((decimal)Math.Pow((double)(1 + monthlyInterestRate), remainingMonths) - 1);
            // Console.WriteLine(monthlyPayment);
            return monthlyPayment;
        }

        public decimal GetNextMonthPayment()
        {
            if (Status == LoanStatus.Completed)
            {
                return 0;
            }
            return PaymentSchedules.Where(p => p.PaidAmount == 0).First().Amount;
        }
      
        public void MakePayment(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Payment amount must be positive", nameof(amount));

            decimal remainingPayment = amount;

            // 1. First, process all penalties
            remainingPayment = ProcessPenalties(remainingPayment);
            if (remainingPayment <= 0) return;
            
            Console.WriteLine("remaining amount:");
            Console.WriteLine(remainingPayment);
            
            RemainingAmount -= amount;
            if (RemainingAmount < 0)
            {
                throw new ArgumentException("amount is bigger than necessary");
            }

            // 2. Check if the remaining amount is approximately equal to the payment amount (within tolerance)
            if (Math.Abs(RemainingAmount) < Tolerance)
            {
                Status = LoanStatus.Completed;
                foreach (var ps in PaymentSchedules)
                {
                    ps.PaidAmount = ps.Amount;
                    ps.IsPaid = true;
                }
                return;
            }

            // 3. Check and process current month's payment
            var currentMonthPayment = GetCurrentMonthPaymentSchedule();
            Console.WriteLine($"current monht {currentMonthPayment!=null} dsadda ");
            if (currentMonthPayment != null)
            {
                remainingPayment = ProcessCurrentMonthPayment(currentMonthPayment, remainingPayment);
            }

            // 4. If there's still money left, recalculate future payments
            if (remainingPayment > 0)
            {
                RecalculateFuturePayments();
            }
        }
        
        public IEnumerable<PaymentSchedule> GeneratePaymentSchedules()
        {
            var schedules = new List<PaymentSchedule>();
            var monthlyPayment = InitialMonthlyPayment(); 

            for (int month = 1; month <= DurationInMonths; month++)
            {
                var paymentDate = StartDate.AddMonths(month); 
                var schedule = new PaymentSchedule
                {
                    LoanId = Id,
                    PaymentDay = paymentDate,
                    Amount = monthlyPayment,
                    IsPaid = false,
                    Loan = this
                };

                schedules.Add(schedule);
            }
            PaymentSchedules = schedules;
            return schedules;
        }

        public decimal GetThisMonthPayment()
        {
            var currentMonthPayment = GetCurrentMonthPaymentSchedule();
            if (currentMonthPayment == null)
            {
                return 0;
            }

            return currentMonthPayment.IsPaid ? 0 : currentMonthPayment.Amount - currentMonthPayment.PaidAmount;
        }
        
        public decimal CalculatePenalty()
        {
            return Amount * (decimal)0.01;
        }
        
        
        private DateTime GetLastMonthPaymentDay(int paymentDay)
        {
            var now = DateTime.Now;

            DateTime lastMonthPaymentDay;

            if (now.Day >= paymentDay)
            {
                lastMonthPaymentDay = new DateTime(now.Year, now.Month, paymentDay);
            }
            else
            {
                var lastMonth = now.AddMonths(-1);
                int daysInLastMonth = DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month);

                lastMonthPaymentDay = paymentDay > daysInLastMonth
                    ? new DateTime(lastMonth.Year, lastMonth.Month, daysInLastMonth)
                    : new DateTime(lastMonth.Year, lastMonth.Month, paymentDay);
            }

            return lastMonthPaymentDay;
        }
        
        private decimal InitialMonthlyPayment()
        {
            if (Amount <= 0) throw new InvalidOperationException("Remaining amount must be greater than 0.");
        
            var monthlyRate = InterestRate / 100 / 12;
            
        
            if (DurationInMonths <= 0) throw new InvalidOperationException("Remaining months must be greater than 0.");
        
            return Amount * monthlyRate / (1 - (decimal)Math.Pow(1 + (double)monthlyRate, -DurationInMonths));
        }
        
        private decimal ProcessPenalties(decimal amount)
        {
            foreach (var penalty in Penalties.Where(p => !p.IsPaid).OrderBy(p => p.ImposedDate))
            {
                if (amount <= 0) break;

                decimal penaltyPayment = Math.Min(penalty.RemainingAmount, amount);
                penalty.RemainingAmount -= penaltyPayment;
                penalty.IsPaid = penalty.RemainingAmount <= 0;
                amount -= penaltyPayment;
            }
            return amount;
        }
        
        private PaymentSchedule? GetCurrentMonthPaymentSchedule()
        {
            DateTime today = DateTime.Now;
            DateTime checkMonth = today.Day < StartDate.Day ? today : today.AddMonths(1);
    
            return PaymentSchedules
                .Where(p => !p.IsPaid && 
                            p.PaymentDay.Month == checkMonth.Month && 
                            p.PaymentDay.Year == checkMonth.Year)
                .MinBy(p => p.PaymentDay);
        }
        private decimal ProcessCurrentMonthPayment(PaymentSchedule currentPayment, decimal amount)
        {
            decimal remainingPayment = currentPayment.Amount - currentPayment.PaidAmount;

            if (remainingPayment <= 0)
            {
                currentPayment.IsPaid = true;
                return amount; 
            }

            if (amount >= remainingPayment)
            {
                currentPayment.PaidAmount += remainingPayment;
                currentPayment.IsPaid = true;
                return amount - remainingPayment;
            }
    
            currentPayment.PaidAmount += amount;
            return 0;
        }
        
        private void RecalculateFuturePayments()
        {
            decimal newMonthlyAmount = CalculateMonthlyPayment();
            foreach (var schedule in PaymentSchedules.Where(p => !p.IsPaid))
            {
                schedule.Amount = newMonthlyAmount;
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
    
    
    
    // private int CalculateRemainingMonth()
    // {
    //     var now = DateTime.Now;
    //     if (now > EndDate) return 0;  // If current date is past the loan end date, return 0
    //     var months = ((EndDate.Year - now.Year) * 12) + EndDate.Month - now.Month;
    //     return months;
    // }
    //
    // public decimal CalculateTotalPayment()
    // {
    //     var monthlyPayment = CalculateMonthlyPayment();
    //     int remainingMonths = CalculateRemainingMonth();
    //
    //     return monthlyPayment * remainingMonths;
    // }
    //
   
    //
    // public bool HavePaidFullyLastMonth()
    // {
    //     var now = DateTime.Now;
    //     var paymentDay = StartDate.Day;
    //     
    //     DateTime lastPaymentPeriodStart;
    //     DateTime lastPaymentPeriodEnd;
    //
    //     if (now.Day < paymentDay)
    //     {
    //         lastPaymentPeriodEnd = new DateTime(now.Year, now.Month, paymentDay).AddDays(-1);
    //         lastPaymentPeriodStart = lastPaymentPeriodEnd.AddMonths(-1).AddDays(1);
    //     }
    //     else
    //     {
    //         lastPaymentPeriodStart = new DateTime(now.Year, now.Month, paymentDay);
    //         lastPaymentPeriodEnd = lastPaymentPeriodStart.AddMonths(1).AddDays(-1);
    //     }
    //
    //     decimal expectedPayment = InitialMonthlyPayment();
    //
    //     decimal paymentsMade = Payments
    //         .Where(p => p.PaymentDate >= lastPaymentPeriodStart && p.PaymentDate <= lastPaymentPeriodEnd)
    //         .Sum(p => p.Amount);
    //
    //     bool penaltiesPaid = Penalties.All(p => p.IsPaid);
    //
    //     return paymentsMade >= expectedPayment && penaltiesPaid;
    // }
    //

}

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
        public void MakePayment(decimal amount)
        {
            RemainingAmount -= amount;
            
            foreach (var penalty in Penalties.Where(p => !p.IsPaid).OrderBy(p => p.ImposedDate))
            {
                if (amount <= 0) break;

                decimal remainingPenalty = penalty.RemainingAmount;

                if (amount >= remainingPenalty)
                {
                    penalty.RemainingAmount = 0;
                    penalty.IsPaid = true;
                    amount -= remainingPenalty;
                }
                else
                {
                    penalty.RemainingAmount -= amount;
                    amount = 0;
                }
            }

            // foreach (var paymentSchedule in PaymentSchedules.Where(p => !p.IsPaid).OrderBy(p => p.PaymentDay))
            // {
            //     if (amount <= 0) break;
            //     Console.WriteLine("Im here");
            //     
            //     decimal remainingPayment = paymentSchedule.Amount - paymentSchedule.PaidAmount;
            //
            //     if (amount >= remainingPayment)
            //     {
            //         Console.WriteLine("Im here");
            //         paymentSchedule.PaidAmount += remainingPayment;
            //         paymentSchedule.IsPaid = true;
            //         amount -= remainingPayment;
            //     }
            //     else
            //     {
            //         paymentSchedule.PaidAmount += amount;
            //         amount = 0;
            //     }
            // }
            // Identify the current month payment schedule
            DateTime today = DateTime.Now;
            DateTime checkMonth = today.Day < StartDate.Day ? today : today.AddMonths(1);
            
            var currentMonthPayment = PaymentSchedules
                .Where(p => !p.IsPaid && p.PaymentDay.Month == checkMonth.Month && p.PaymentDay.Year == checkMonth.Year)
                .MinBy(p => p.PaymentDay);

            if (currentMonthPayment != null)
            {
                // Process only the current month's payment
                decimal remainingPayment = currentMonthPayment.Amount - currentMonthPayment.PaidAmount;

                if (remainingPayment <= 0)
                {
                    // Payment has already been made for this month, skip
                    Console.WriteLine("Current month payment is already made.");
                }
                else
                {
                    // Pay the remaining balance for the current month
                    if (amount >= remainingPayment)
                    {
                        currentMonthPayment.PaidAmount += remainingPayment;
                        currentMonthPayment.IsPaid = true;
                        amount -= remainingPayment;
                    }
                    else
                    {
                        currentMonthPayment.PaidAmount += amount;
                        amount = 0;
                    }
                }
            }
            Console.WriteLine(amount);

            if (amount > 0)
            {
                foreach (var ps in PaymentSchedules.Where(p=>!p.IsPaid))
                {
                    ps.Amount = CalculateMonthlyPayment();
                }
            }
        }
        
        public IEnumerable<PaymentSchedule> GeneratePaymentSchedules()
        {
            var schedules = new List<PaymentSchedule>();
            var monthlyPayment = InitialMonthlyPayment(); // Assuming this method calculates the fixed monthly payment

            for (int month = 1; month <= DurationInMonths; month++)
            {
                Console.WriteLine("monthly payement is " + monthlyPayment);
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
                

                Console.WriteLine(schedule.Amount);
            }
            PaymentSchedules = schedules;
            return schedules;
        }

        public decimal GetThisMonthPayment()
        {
            DateTime today = DateTime.Now;
            DateTime checkMonth = today.Day < StartDate.Day ? today : today.AddMonths(1);
            var currentMonthPayment = PaymentSchedules
                .Where(p => !p.IsPaid && p.PaymentDay.Month == checkMonth.Month && p.PaymentDay.Year == checkMonth.Year)
                .MinBy(p => p.PaymentDay);

            if (currentMonthPayment == null)
            {
                return 0;
            }

            return currentMonthPayment.IsPaid ? 0 : currentMonthPayment.Amount - currentMonthPayment.RemainingAmount;
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
        public decimal CalculatePenalty()
        {
            return Amount * (decimal)0.01;
        }
        //
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
        private decimal InitialMonthlyPayment()
        {
            if (Amount <= 0) throw new InvalidOperationException("Remaining amount must be greater than 0.");
        
            var monthlyRate = InterestRate / 100 / 12;
            
        
            if (DurationInMonths <= 0) throw new InvalidOperationException("Remaining months must be greater than 0.");
        
            return Amount * monthlyRate / (1 - (decimal)Math.Pow(1 + (double)monthlyRate, -DurationInMonths));
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

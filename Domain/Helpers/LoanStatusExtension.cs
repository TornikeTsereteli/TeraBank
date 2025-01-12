using Domain.Entities;

namespace Domain.Helpers;

public static class LoanStatusExtension
{
    public static string ToDisplayString(this LoanStatus status)
    {
        return status switch
        {
            LoanStatus.Pending => "Loan is Pending",
            LoanStatus.Approved => "Loan has been Approved",
            LoanStatus.Rejected => "Loan has been Rejected",
            LoanStatus.Completed => "Loan has been Completed",
            _ => "Unknown Status"
        };
    }
}
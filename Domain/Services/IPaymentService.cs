using Domain.Entities;

namespace Domain.Services;

public interface IPaymentService
{
    Task MakePayment(decimal amount, Guid loadId);
    Task MakePenaltyPayment(decimal amount, Guid penaltyId);

    Task<IEnumerable<Payment>> GetAllPayments(Client client);
}
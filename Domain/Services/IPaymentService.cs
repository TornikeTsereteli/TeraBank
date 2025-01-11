using Domain.Entities;

namespace Domain.Services;

public interface IPaymentService
{
    Task MakePayment(int amount, Guid loadId);
    Task GetAllPayments(Client client);
}
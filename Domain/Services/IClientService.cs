using Domain.Entities;
using Domain.Interfaces;

namespace Domain.Services;

public interface IClientService
{
   Task AddClientAsync(Client client);

   Task<Client?> GetClientByUserIdWithLoansAndPaymentsAsync(string userId);
   Task<Client?> GetClientByUserIdWithLoansAsync(string userId);

   Task<Client?> GetClientByUserIdWithLoansAndPenaltiesAsync(string userId);
   Task<Client?> GetClientByUserIdAsync(string userId);

}
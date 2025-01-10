using Domain.Entities;
using Domain.Interfaces;

namespace Domain.Services;

public interface IClientService
{
   Task AddClientAsync(Client client);

}
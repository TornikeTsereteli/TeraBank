using Domain.Entities;
using Domain.Interfaces;
using Domain.Services;

namespace Application.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;

    public ClientService(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task AddClientAsync(Client client)
    {
       await _clientRepository.AddAsync(client);
    }

    public async Task UpdateClientAsync(Client client)
    {
        await _clientRepository.UpdateAsync(client);
    }
}
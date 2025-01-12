using Domain.Entities;
using Domain.Interfaces;
using Domain.Services;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly UserManager<AppUser> _userManager;

    public ClientService(IClientRepository clientRepository, UserManager<AppUser> userManager)
    {
        _clientRepository = clientRepository;
        _userManager = userManager;
    }

    public async Task AddClientAsync(Client client)
    {
       await _clientRepository.AddAsync(client);
    }


    public async Task<Client?> GetClientByUserIdWithLoansAndPaymentsAsync(string userId)
    {
        var user = await _userManager.Users
            .Include(u => u.Client)
            .ThenInclude(c => c.Loans)
            .ThenInclude(l => l.Payments)
            .FirstOrDefaultAsync(u => u.Id == userId);
        return user?.Client;
    }
    
    public async Task<Client?> GetClientByUserIdWithLoansAsync(string userId)
    {
        var user = await _userManager.Users
            .Include(u => u.Client)
            .ThenInclude(c => c.Loans)
            .FirstOrDefaultAsync(u => u.Id == userId);
        return user?.Client;
    }

    public async Task<Client?> GetClientByUserIdWithLoansAndPenalties(string userId)
    {
        var user = await _userManager.Users
            .Include(u => u.Client)
            .ThenInclude(c => c.Loans)
            .ThenInclude(l => l.Penalties)
            .FirstOrDefaultAsync(u => u.Id == userId);
        return user?.Client;
    }

    public async Task<Client?> GetClientByUserIdAsync(string userId)
    {
        var user = await _userManager.Users
            .Include(u => u.Client)
            .FirstOrDefaultAsync(u => u.Id == userId);
        return user?.Client;
    }


    
    
    
}
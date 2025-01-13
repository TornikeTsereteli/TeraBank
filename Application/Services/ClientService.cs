using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Services;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<ClientService> _logger;

    public ClientService(IClientRepository clientRepository, UserManager<AppUser> userManager, ILogger<ClientService> logger)
    {
        _clientRepository = clientRepository;
        _userManager = userManager;
        _logger = logger;
    }

    // simple  AddCLIENT METHOD 
    public async Task AddClientAsync(Client client)
    {
        try
        {
            _logger.LogInformation("Attempting to add a new client.");
            await _clientRepository.AddAsync(client);
            _logger.LogInformation("Client successfully added with ID: {ClientId}", client.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding a client.");
            throw;
        }
    }

    // different type of joins, this method return the Client with Loans and payments are joined
    public async Task<Client?> GetClientByUserIdWithLoansAndPaymentsAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Fetching client data for user with ID: {UserId}. Including loans and payments.", userId);
            var user = await _userManager.Users
                .Include(u => u.Client)
                .ThenInclude(c => c.Loans)
                .ThenInclude(l => l.Payments)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning("No user found with ID: {UserId}", userId);
                return null;
            }

            _logger.LogInformation("Successfully fetched client data for user with ID: {UserId}.", userId);
            return user.Client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching client data for user with ID: {UserId}.", userId);
            throw;
        }
    }
    
    // this method returns client with loans joined

    public async Task<Client?> GetClientByUserIdWithLoansAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Fetching client data for user with ID: {UserId}. Including loans.", userId);
            var user = await _userManager.Users
                .Include(u => u.Client)
                .ThenInclude(c => c.Loans)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning("No user found with ID: {UserId}", userId);
                return null;
            }

            _logger.LogInformation("Successfully fetched client data for user with ID: {UserId}.", userId);
            return user.Client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching client data for user with ID: {UserId}.", userId);
            throw;
        }
    }

    // return client with loans and the penalties joined
    public async Task<Client?> GetClientByUserIdWithLoansAndPenaltiesAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Fetching client data for user with ID: {UserId}. Including loans and penalties.", userId);
            var user = await _userManager.Users
                .Include(u => u.Client)
                .ThenInclude(c => c.Loans)
                .ThenInclude(l => l.Penalties)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning("No user found with ID: {UserId}", userId);
                return null;
            }

            _logger.LogInformation("Successfully fetched client data for user with ID: {UserId}.", userId);
            return user.Client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching client data for user with ID: {UserId}.", userId);
            throw;
        }
    }

    // just returns client without joined anything 
    public async Task<Client?> GetClientByUserIdAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Fetching client data for user with ID: {UserId}.", userId);
            var user = await _userManager.Users
                .Include(u => u.Client)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning("No user found with ID: {UserId}", userId);
                return null;
            }

            _logger.LogInformation("Successfully fetched client data for user with ID: {UserId}.", userId);
            return user.Client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching client data for user with ID: {UserId}.", userId);
            throw;
        }
    }
}

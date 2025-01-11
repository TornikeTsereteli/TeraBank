using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _dbContext;
        
        public ClientRepository(ApplicationDbContext applicationDbContext)
        {
            _dbContext = applicationDbContext;
        }
        
        public async Task<Client> GetByIdAsync(Guid id)
        {
            return await _dbContext.Clients
                .FirstOrDefaultAsync(client => client.Id.Equals(id)) ?? throw new InvalidOperationException("no such client exists");
        }

        public async Task<IEnumerable<Client>> GetAllAsync()
        {
            return await _dbContext.Clients.ToListAsync();
        }

        public async Task AddAsync(Client entity)
        {
            await _dbContext.Clients.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Client entity)
        {
            _dbContext.Clients.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var client = await _dbContext.Clients.FindAsync(id);
            if (client != null)
            {
                _dbContext.Clients.Remove(client);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
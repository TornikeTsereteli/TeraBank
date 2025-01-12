using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PenaltyRepository : IPenaltyRepository
{
    private readonly ApplicationDbContext _context;

    public PenaltyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Penalty> GetByIdAsync(Guid id)
    {
        return await _context.Set<Penalty>().FindAsync(id);
    }

    public async Task<IEnumerable<Penalty>> GetAllAsync()
    {
        return await _context.Set<Penalty>().ToListAsync();
    }

    public async Task AddAsync(Penalty entity)
    {
        await _context.Set<Penalty>().AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Penalty entity)
    {
        _context.Set<Penalty>().Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.Set<Penalty>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
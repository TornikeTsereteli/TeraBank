using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Interfaces.Repositories;
using Infrastructure.Database;

namespace Infrastructure.Repositories
{
    public class LoanRepository : ILoanRepository
    {
        private readonly ApplicationDbContext _context;

        public LoanRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Loan> GetByIdAsync(Guid id)
        {
            return await _context.Set<Loan>().FindAsync(id);
        }

        public async Task<IEnumerable<Loan>> GetAllAsync()
        {
            return await _context.Set<Loan>().ToListAsync();
        }

        public async Task AddAsync(Loan entity)
        {
            await _context.Set<Loan>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Loan entity)
        {
            _context.Set<Loan>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Set<Loan>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
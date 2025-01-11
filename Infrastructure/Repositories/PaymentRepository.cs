using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Database;

namespace Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> GetByIdAsync(Guid id)
        {
            return await _context.Set<Payment>().FindAsync(id);
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Set<Payment>().ToListAsync();
        }

        public async Task AddAsync(Payment entity)
        {
            await _context.Set<Payment>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Payment entity)
        {
            _context.Set<Payment>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Set<Payment>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
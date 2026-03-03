using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;
    public RefreshTokenRepository(AppDbContext context) => _context = context;

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(string userId)
    {
        return await _context.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();
    }

    public async Task AddAsync(RefreshToken token)
    {
        await _context.RefreshTokens.AddAsync(token);
    }

    public void Update(RefreshToken token) => _context.RefreshTokens.Update(token);

    public void UpdateRange(IEnumerable<RefreshToken> tokens) => _context.RefreshTokens.UpdateRange(tokens);
}

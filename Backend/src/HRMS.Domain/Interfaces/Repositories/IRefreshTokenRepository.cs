using HRMS.Domain.Entities;

namespace HRMS.Domain.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(string userId);
    Task AddAsync(RefreshToken token);
    void Update(RefreshToken token);
    void UpdateRange(IEnumerable<RefreshToken> tokens);
}

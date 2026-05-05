using Microsoft.EntityFrameworkCore;
using PMS.Core.Abstractions;
using PMS.Core.Entities;
using PMS.Data;

namespace PMS.Services;

public sealed class UserService : IUserService
{
    private readonly PharmacyDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IClock _clock;

    public UserService(PharmacyDbContext db, IPasswordHasher hasher, IClock clock)
    {
        _db = db;
        _hasher = hasher;
        _clock = clock;
    }

    public Task<User?> GetByIdAsync(int id)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task<User?> GetByUsernameAsync(string username)
        => _db.Users.FirstOrDefaultAsync(u =>
            u.Username == username && u.IsActive);

    public async Task<int> CreateAsync(User user, string plainPassword, int? createdBy)
    {
        var (hash, salt) = _hasher.Hash(plainPassword);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.SaveDate = _clock.UtcNow;
        user.SavedBy = createdBy;
        user.LastPasswordChangeUtc = _clock.UtcNow;

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user.Id;
    }

    public async Task ChangePasswordAsync(int userId, string newPassword, int changedBy)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found.");
        var (hash, salt) = _hasher.Hash(newPassword);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.LastPasswordChangeUtc = _clock.UtcNow;
        user.MustChangePassword = false;
        user.UpdatedBy = changedBy;
        user.UpdatedDate = _clock.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task SetLockedAsync(int userId, bool locked, int updatedBy)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return;
        user.IsLocked = locked;
        user.UpdatedBy = updatedBy;
        user.UpdatedDate = _clock.UtcNow;
        await _db.SaveChangesAsync();
    }
}

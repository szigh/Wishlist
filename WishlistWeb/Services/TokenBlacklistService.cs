using System.Collections.Concurrent;

namespace WishlistWeb.Services
{
    public interface ITokenBlacklistService
    {
        void BlacklistToken(string tokenId, DateTime expiration);
        bool IsTokenBlacklisted(string tokenId);
        void CleanupExpiredTokens();
    }

    public class TokenBlacklistService : ITokenBlacklistService
    {
        // ConcurrentDictionary is thread-safe for multiple simultaneous requests
        // Key: token JTI (unique token ID), Value: expiration time
        private readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens = new();
        private DateTime _lastCleanup = DateTime.UtcNow;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);

        public void BlacklistToken(string tokenId, DateTime expiration)
        {
            _blacklistedTokens.TryAdd(tokenId, expiration);
            
            // Periodic cleanup to prevent memory buildup
            if (DateTime.UtcNow - _lastCleanup > _cleanupInterval)
            {
                CleanupExpiredTokens();
            }
        }

        public bool IsTokenBlacklisted(string tokenId)
        {
            return _blacklistedTokens.ContainsKey(tokenId);
        }

        public void CleanupExpiredTokens()
        {
            var now = DateTime.UtcNow;
            var expiredTokens = _blacklistedTokens
                .Where(kvp => kvp.Value < now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var tokenId in expiredTokens)
            {
                _blacklistedTokens.TryRemove(tokenId, out _);
            }

            _lastCleanup = now;
        }
    }
}

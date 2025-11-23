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
        private long _lastCleanupTicks = DateTime.UtcNow.Ticks;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);

        public void BlacklistToken(string tokenId, DateTime expiration)
        {
            _blacklistedTokens.TryAdd(tokenId, expiration);
            
            // Periodic cleanup to prevent memory buildup
            var now = DateTime.UtcNow;
            var lastCleanup = new DateTime(Interlocked.Read(ref _lastCleanupTicks));
            
            if (now - lastCleanup > _cleanupInterval)
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
            var lastCleanup = new DateTime(Interlocked.Read(ref _lastCleanupTicks));
            
            // Check if another thread already performed cleanup recently
            if (now - lastCleanup <= _cleanupInterval)
            {
                return;
            }
            
            // Try to claim the cleanup operation atomically
            var expectedTicks = lastCleanup.Ticks;
            var newTicks = now.Ticks;
            if (Interlocked.CompareExchange(ref _lastCleanupTicks, newTicks, expectedTicks) != expectedTicks)
            {
                // Another thread already started cleanup
                return;
            }
            
            // Perform cleanup - only one thread will reach this point
            var expiredTokens = _blacklistedTokens
                .Where(kvp => kvp.Value < now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var tokenId in expiredTokens)
            {
                _blacklistedTokens.TryRemove(tokenId, out _);
            }
        }
    }
}

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
            // Use Interlocked.Read for thread-safe read of 64-bit value
            var nowTicks = DateTime.UtcNow.Ticks;
            var lastCleanupTicks = Interlocked.Read(ref _lastCleanupTicks);
            
            if (nowTicks - lastCleanupTicks > _cleanupInterval.Ticks)
            {
                // Atomically claim the cleanup operation
                if (Interlocked.CompareExchange(ref _lastCleanupTicks, nowTicks, lastCleanupTicks) == lastCleanupTicks)
                {
                    CleanupExpiredTokens();
                }
            }
        }

        public bool IsTokenBlacklisted(string tokenId)
        {
            return _blacklistedTokens.ContainsKey(tokenId);
        }

        public void CleanupExpiredTokens()
        {
            var now = DateTime.UtcNow;
            var nowTicks = now.Ticks;
            
            // Read the last cleanup time immediately before the compare-exchange
            var lastCleanupTicks = Interlocked.Read(ref _lastCleanupTicks);
            
            // Check if another thread already performed cleanup recently
            if (nowTicks - lastCleanupTicks <= _cleanupInterval.Ticks)
            {
                return;
            }
            
            // Try to claim the cleanup operation atomically
            if (Interlocked.CompareExchange(ref _lastCleanupTicks, nowTicks, lastCleanupTicks) != lastCleanupTicks)
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

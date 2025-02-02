﻿using System.Collections.Concurrent;

namespace RateLimiting
{
    public class SlidingWindowRateLimiter : RateLimiter
    {
        private readonly ConcurrentDictionary<long, int> _windowCount = new();
        private readonly object _lock = new();

        public SlidingWindowRateLimiter(int maxRequestNum, int period)
            : base(maxRequestNum, period)
        {
        }

        public override bool IsAllow()
        {
            var currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var currentWindowStart = currentTime / (Period * 1000) * (Period * 1000);
            var previousWindowStart = currentWindowStart - (Period * 1000);

            _windowCount.TryAdd(currentWindowStart, 0);
            _windowCount.TryGetValue(previousWindowStart, out var windowCount);
            var previousPecentage = 1 - (currentTime - currentWindowStart) / (double)(Period * 1000);

            lock (_lock)
            {
                var capacity = MaxRequestNum - windowCount * previousPecentage - _windowCount[currentWindowStart];

                if (capacity > 1)
                {
                    _windowCount[currentWindowStart]++;
                    return true;
                }
            }

            return false;
        }
    }
}
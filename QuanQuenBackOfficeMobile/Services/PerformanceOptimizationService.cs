using System.Diagnostics;

namespace QuanQuenBackOfficeMobile.Services
{
    public interface IPerformanceOptimizationService
    {
        Task RunOnBackgroundThreadAsync(Func<Task> operation, string operationName = "");
        Task<T> RunOnBackgroundThreadAsync<T>(Func<Task<T>> operation, string operationName = "");
        void RunOnMainThread(Action action);
        Task RunOnMainThreadAsync(Func<Task> action);
        void OptimizeMemoryUsage();
        void ForceGarbageCollection();
    }

    public class PerformanceOptimizationService : IPerformanceOptimizationService
    {
        private readonly SemaphoreSlim _semaphore = new(Environment.ProcessorCount, Environment.ProcessorCount);

        public async Task RunOnBackgroundThreadAsync(Func<Task> operation, string operationName = "")
        {
            await _semaphore.WaitAsync();
            try
            {
                await Task.Run(async () =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    try
                    {
                        await operation();
                    }
                    finally
                    {
                        stopwatch.Stop();
                        if (!string.IsNullOrEmpty(operationName))
                        {
                            Debug.WriteLine($"[Performance] {operationName} completed in {stopwatch.ElapsedMilliseconds}ms");
                        }
                    }
                });
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<T> RunOnBackgroundThreadAsync<T>(Func<Task<T>> operation, string operationName = "")
        {
            await _semaphore.WaitAsync();
            try
            {
                return await Task.Run(async () =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    try
                    {
                        return await operation();
                    }
                    finally
                    {
                        stopwatch.Stop();
                        if (!string.IsNullOrEmpty(operationName))
                        {
                            Debug.WriteLine($"[Performance] {operationName} completed in {stopwatch.ElapsedMilliseconds}ms");
                        }
                    }
                });
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void RunOnMainThread(Action action)
        {
            if (MainThread.IsMainThread)
            {
                action();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(action);
            }
        }

        public async Task RunOnMainThreadAsync(Func<Task> action)
        {
            if (MainThread.IsMainThread)
            {
                await action();
            }
            else
            {
                await MainThread.InvokeOnMainThreadAsync(action);
            }
        }

        public void OptimizeMemoryUsage()
        {
            // Giảm memory pressure bằng cách:
            // 1. Clear unused collections
            // 2. Dispose unused resources
            // 3. Force garbage collection nếu cần
            
            Debug.WriteLine("[Performance] Optimizing memory usage...");
            
            // Force GC nếu memory usage cao
            var memoryBefore = GC.GetTotalMemory(false);
            if (memoryBefore > 100 * 1024 * 1024) // 100MB
            {
                ForceGarbageCollection();
                var memoryAfter = GC.GetTotalMemory(false);
                Debug.WriteLine($"[Performance] Memory optimized: {memoryBefore / 1024 / 1024}MB -> {memoryAfter / 1024 / 1024}MB");
            }
        }

        public void ForceGarbageCollection()
        {
            Debug.WriteLine("[Performance] Forcing garbage collection...");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
        }
    }
}

using System;
using System.Buffers;
using System.Threading.Tasks;

namespace Performance.Optimization
{
    /// <summary>
    /// High-performance data processing engine for task B-002.
    /// Implements buffer pooling to eliminate LOH fragmentation.
    /// </summary>
    public class InboundDataProcessor
    {
        // Use shared memory pool to reduce heap allocations and GC pressure.
        private static readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;

        /// <summary>
        /// Processes inbound data using pooled buffers.
        /// </summary>
        /// <param name="inputData">The source memory block to process.</param>
        public async Task ProcessDataAsync(ReadOnlyMemory<byte> inputData)
        {
            // Optimization: Rent a buffer instead of allocating 'new byte[128KB]'.
            // 128KB triggers Large Object Heap (LOH), leading to expensive Stop-the-world GC events.
            int bufferSize = 131072; 
            byte[] buffer = _bufferPool.Rent(bufferSize);

            try
            {
                // Perform high-speed data copy and processing.
                inputData.Span.CopyTo(buffer);
                
                await PerformHeavyComputationAsync(buffer);
            }
            catch (Exception ex)
            {
                // Error handling with context.
                Console.WriteLine($"Processing error: {ex.Message}");
                throw;
            }
            finally
            {
                // Critical: Ensure buffer is returned to the pool to prevent leaks.
                _bufferPool.Return(buffer);
            }
        }

        private Task PerformHeavyComputationAsync(byte[] data)
        {
            // Simulate complex business logic computation.
            return Task.Delay(10); 
        }
    }
}

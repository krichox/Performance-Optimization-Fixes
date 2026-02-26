using System;
using System.Buffers;
using System.Threading.Tasks;

namespace Performance.Optimization
{
    /// <summary>
    /// 高性能数据处理引擎 - 针对 B-002 内存泄漏修复
    /// </summary>
    public class InboundDataProcessor
    {
        // 使用共享内存池减少堆分配，彻底消除 LOH 碎片
        private static readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;

        public async Task ProcessDataAsync(ReadOnlyMemory<byte> inputData)
        {
            // 💡 优化点：不再使用 new byte[128KB]，而是从池中租用
            // 128KB 是进入 LOH 的阈值，频繁分配会导致严重的 GC Stop-the-world 停顿
            int bufferSize = 131072; 
            byte[] buffer = _bufferPool.Rent(bufferSize);

            try
            {
                // 模拟高性能数据拷贝与处理
                inputData.Span.CopyTo(buffer);
                
                await PerformHeavyComputationAsync(buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理异常: {ex.Message}");
                throw;
            }
            finally
            {
                // ⚠️ 关键闭环：必须归还缓冲区，否则会导致池内存泄漏
                _bufferPool.Return(buffer);
            }
        }

        private Task PerformHeavyComputationAsync(byte[] data)
        {
            // 模拟复杂的业务处理
            return Task.Delay(10); 
        }
    }
}

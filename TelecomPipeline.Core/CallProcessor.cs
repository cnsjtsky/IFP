using System;
using System.Threading;

namespace TelecomPipeline.Core
{
    public static class CallProcessor
    {
        public static decimal ProcessCallsSequential(CallRecord[] records)
        {
            if (records == null) throw new ArgumentNullException(nameof(records));
            if (records.Length == 0) return 0m;

            decimal total = 0m;
            foreach (ref readonly var record in records.AsSpan())
            {
                total += CallPricing.CalculateCost(in record);
            }
            return total;
        }

        public static decimal ProcessCallsParallel(CallRecord[] records)
        {
            if (records == null) throw new ArgumentNullException(nameof(records));
            if (records.Length == 0) return 0m;
            if (records.Length % 2 != 0)
                throw new ArgumentException("Array length must be even for balanced 2-thread splitting.", nameof(records));

            int mid = records.Length / 2;

            CallRecord[] part1 = records[..mid];
            CallRecord[] part2 = records[mid..];

            decimal[] output1 = new decimal[part1.Length];
            decimal[] output2 = new decimal[part2.Length];

            Exception? worker1Exception = null;
            Exception? worker2Exception = null;

            Thread thread1 = new Thread(() =>
            {
                try
                {
                    for (int i = 0; i < part1.Length; i++)
                    {
                        output1[i] = CallPricing.CalculateCost(in part1[i]);
                    }
                }
                catch (Exception ex)
                {
                    worker1Exception = ex;
                }
            });

            Thread thread2 = new Thread(() =>
            {
                try
                {
                    for (int i = 0; i < part2.Length; i++)
                    {
                        output2[i] = CallPricing.CalculateCost(in part2[i]);
                    }
                }
                catch (Exception ex)
                {
                    worker2Exception = ex;
                }
            });

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();

            if (worker1Exception != null) throw new AggregateException("Worker thread 1 failed.", worker1Exception);
            if (worker2Exception != null) throw new AggregateException("Worker thread 2 failed.", worker2Exception);

            decimal totalSum = 0m;
            foreach (decimal cost in output1) totalSum += cost;
            foreach (decimal cost in output2) totalSum += cost;

            return totalSum;
        }
    }
}
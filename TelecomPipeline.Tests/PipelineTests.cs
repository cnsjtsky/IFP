using System;
using TelecomPipeline.Core;
using Xunit;

namespace TelecomPipeline.Tests
{
    public class PipelineTests
    {
        [Theory]
        [InlineData("KZ", false, 4.0, 60.00)]
        [InlineData("KZ", true, 0.5, 50.00)]
        [InlineData("US", true, 10.0, 1200.00)]
        [InlineData("DE", false, 3.0, 135.00)]
        [InlineData("XX", false, 2.0, 90.00)]
        [InlineData("KZ", true, 1.0, 45.00)]
        public void GroupA_TariffCorrectness(string country, bool isRoaming, double duration, decimal expectedCost)
        {
            var record = new CallRecord("REC1", country, duration, isRoaming);
            decimal actual = CallPricing.CalculateCost(in record);
            Assert.Equal(expectedCost, actual);
        }

        [Fact]
        public void GroupB_ConstructorValidation_And_DefaultRecord()
        {
            Assert.Throws<ArgumentException>(() => new CallRecord("", "KZ", 5.0, false));
            Assert.Throws<ArgumentException>(() => new CallRecord("R1", "", 5.0, false));
            Assert.Throws<ArgumentException>(() => new CallRecord("R1", "KZ", -1.0, false));
            Assert.Throws<ArgumentException>(() => new CallRecord("R1", "KZ", double.NaN, false));
            Assert.Throws<ArgumentException>(() => new CallRecord("R1", "KZ", double.PositiveInfinity, false));
            Assert.Throws<ArgumentException>(() => new CallRecord("R1", "KZ", 10001.0, false));

            CallRecord defaultRecord = default;
            Assert.Throws<ArgumentException>(() => CallPricing.CalculateCost(in defaultRecord));
        }

        [Fact]
        public void GroupC_SequentialAndParallelAgreement_100Runs()
        {
            int size = 1000;
            var records = new CallRecord[size];
            for (int i = 0; i < size; i++)
            {
                records[i] = new CallRecord($"ID_{i}", (i % 2 == 0) ? "KZ" : "US", (i % 15) + 0.5, i % 3 == 0);
            }

            decimal sequentialTotal = CallProcessor.ProcessCallsSequential(records);

            for (int run = 0; run < 100; run++)
            {
                decimal parallelTotal = CallProcessor.ProcessCallsParallel(records);
                Assert.Equal(sequentialTotal, parallelTotal);
            }

            Assert.Equal(0m, CallProcessor.ProcessCallsParallel(Array.Empty<CallRecord>()));
            Assert.Throws<ArgumentException>(() => CallProcessor.ProcessCallsParallel(new CallRecord[3]));
        }
    }
}
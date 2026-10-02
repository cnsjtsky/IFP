using System;

namespace TelecomPipeline.Core
{
    public static class CallPricing
    {
        public static decimal CalculateCost(in CallRecord record)
        {
            if (!record.IsValid)
                throw new ArgumentException("Invalid CallRecord instance or default value passed.", nameof(record));

            decimal rawCost = record switch
            {
                { DurationMinutes: var d } when double.IsNaN(d) || double.IsInfinity(d) || d < 0 || d > 10000 =>
                    throw new ArgumentException("Invalid call duration in pricing calculation."),

                { IsRoaming: true, DestinationCountry: "KZ", DurationMinutes: < 1.0 } => 50.00m,

                { IsRoaming: false, DestinationCountry: "KZ" } => (decimal)record.DurationMinutes * 15.00m,

                { IsRoaming: true, DurationMinutes: >= 10.0 } => (decimal)record.DurationMinutes * 120.00m,

                _ => (decimal)record.DurationMinutes * 45.00m
            };

            return Math.Round(rawCost, 2, MidpointRounding.AwayFromZero);
        }
    }
}
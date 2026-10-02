using System;

namespace TelecomPipeline.Core
{
    public readonly record struct CallRecord
    {
        public string RecordId { get; }
        public string DestinationCountry { get; }
        public double DurationMinutes { get; }
        public bool IsRoaming { get; }

        public CallRecord(string recordId, string destinationCountry, double durationMinutes, bool isRoaming)
        {
            if (string.IsNullOrWhiteSpace(recordId))
                throw new ArgumentException("Record ID cannot be null, empty, or whitespace.", nameof(recordId));

            if (string.IsNullOrWhiteSpace(destinationCountry))
                throw new ArgumentException("Destination country cannot be null, empty, or whitespace.", nameof(destinationCountry));

            if (double.IsNaN(durationMinutes) || double.IsInfinity(durationMinutes) || durationMinutes < 0 || durationMinutes > 10000)
                throw new ArgumentException("Duration must be a finite, non-negative number <= 10,000.", nameof(durationMinutes));

            RecordId = recordId;
            DestinationCountry = destinationCountry;
            DurationMinutes = durationMinutes;
            IsRoaming = isRoaming;
        }

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(RecordId) &&
            !string.IsNullOrWhiteSpace(DestinationCountry) &&
            !double.IsNaN(DurationMinutes) &&
            !double.IsInfinity(DurationMinutes) &&
            DurationMinutes >= 0 &&
            DurationMinutes <= 10000;
    }
}
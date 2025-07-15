using System;
using System.Collections.Generic;
using System.Linq;

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; }
    public string EventType { get; set; }
}

public class Program
{
    public static void Main()
    {
        // Sample log data
        List<LogEntry> logs = new List<LogEntry>
        {
            new LogEntry { Timestamp = DateTime.Parse("2025-06-29 12:01:04"), UserId = "user123", EventType = "API_CALL" },
            new LogEntry { Timestamp = DateTime.Parse("2025-06-29 12:01:07"), UserId = "user456", EventType = "API_CALL" },
            new LogEntry { Timestamp = DateTime.Parse("2025-06-29 12:02:12"), UserId = "user789", EventType = "LOGIN" },
            new LogEntry { Timestamp = DateTime.Parse("2025-06-29 12:02:35"), UserId = "user234", EventType = "API_CALL" },
            new LogEntry { Timestamp = DateTime.Parse("2025-06-29 12:03:01"), UserId = "user567", EventType = "ERROR" },
            new LogEntry { Timestamp = DateTime.Parse("2025-06-29 12:05:10"), UserId = "user888", EventType = "API_CALL" },
            new LogEntry { Timestamp = DateTime.Parse("2025-06-29 12:05:15"), UserId = "user999", EventType = "API_CALL" },
            new LogEntry { Timestamp = DateTime.Parse("2025-06-29 12:05:20"), UserId = "user555", EventType = "LOGIN" },
            new LogEntry { Timestamp = DateTime.Parse("2025-06-29 12:05:25"), UserId = "user777", EventType = "API_CALL" },
            new LogEntry { Timestamp = DateTime.Parse("2025-06-29 12:05:30"), UserId = "user222", EventType = "ERROR" }
        };

        int numberOfBins = 3;

        // Prepare total time range
        DateTime minTime = logs.Min(l => l.Timestamp);
        DateTime maxTime = logs.Max(l => l.Timestamp);
        double totalMinutes = (maxTime - minTime).TotalMinutes;

        var variances = new Dictionary<string, double>();
        var entropyScores = new Dictionary<string, double>();
        var balanceRatios = new Dictionary<string, double>();

        // === Equal Width Binning ===
        Console.WriteLine("\n=== Equal Width Binning ===");
        double binWidth = totalMinutes / numberOfBins;

        var equalWidthBins = logs.GroupBy(log =>
        {
            double minutesFromStart = (log.Timestamp - minTime).TotalMinutes;
            int binIndex = (int)(minutesFromStart / binWidth); // Which bin this log belongs to
            return binIndex;
        }).OrderBy(b => b.Key).ToList();

        foreach (var bin in equalWidthBins.OrderBy(b => b.Key))
        {
            Console.WriteLine($"Bin {bin.Key}: {bin.Count()} logs");
        }

        // === Equal Frequency Binning ===
        Console.WriteLine("\n=== Equal Frequency (Quantile) Binning ===");
        var sortedLogs = logs.OrderBy(log => log.Timestamp).ToList();
        int binSize = logs.Count / numberOfBins;
        var efCounts = new List<int>();

        for (int i = 0; i < numberOfBins; i++)
        {
            var binLogs = sortedLogs.Skip(i * binSize).Take(binSize).ToList();
            DateTime binStart = binLogs.First().Timestamp;
            DateTime binEnd = binLogs.Last().Timestamp;
            Console.WriteLine($"Bin {i + 1} [{binStart:HH:mm:ss} - {binEnd:HH:mm:ss}]: {binLogs.Count} logs");
        }

        // === Business Domain-Based Binning ===
        Console.WriteLine("\n=== Business Domain-Based Binning ===");
        var domainBins = logs.GroupBy(log =>
        {
            if (log.Timestamp.Minute <= 2)
                return "Morning";
            else if (log.Timestamp.Minute <= 4)
                return "Midday";
            else
                return "Afternoon";
        }).OrderBy(b => b.Key).ToList();

        var bdCounts = domainBins.Select(b => b.Count()).ToList();
        foreach (var bin in domainBins)
        {
            Console.WriteLine($"{bin.Key}: {bin.Count()} logs");
        }
    }
}

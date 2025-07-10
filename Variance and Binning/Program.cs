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

        // === Equal Width Binning ===
        Console.WriteLine("\n=== Equal Width Binning ===");
        double binWidth = totalMinutes / numberOfBins;

        var equalWidthBins = logs.GroupBy(log =>
        {
            double minutesFromStart = (log.Timestamp - minTime).TotalMinutes;
            int binIndex = (int)(minutesFromStart / binWidth);
            if (binIndex >= numberOfBins) binIndex = numberOfBins - 1; // put to last bin
            return binIndex;
        }).OrderBy(b => b.Key).ToList();

        var ewCounts = equalWidthBins.Select(b => b.Count()).ToList();
        PrintBinCounts(ewCounts);
        variances["Equal Width"] = CalculateVariance(ewCounts);

        // === Equal Frequency Binning ===
        Console.WriteLine("\n=== Equal Frequency (Quantile) Binning ===");
        var sortedLogs = logs.OrderBy(log => log.Timestamp).ToList();
        int binSize = logs.Count / numberOfBins;
        var efCounts = new List<int>();

        for (int i = 0; i < numberOfBins; i++)
        {
            var binLogs = (i == numberOfBins - 1)
                ? sortedLogs.Skip(i * binSize).ToList() // Last bin takes remaining
                : sortedLogs.Skip(i * binSize).Take(binSize).ToList();

            efCounts.Add(binLogs.Count);
            DateTime binStart = binLogs.First().Timestamp;
            DateTime binEnd = binLogs.Last().Timestamp;
            Console.WriteLine($"Bin {i + 1} [{binStart:HH:mm:ss} - {binEnd:HH:mm:ss}]: {binLogs.Count} logs");
        }
        variances["Equal Frequency"] = CalculateVariance(efCounts);

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
        variances["Business Domain"] = CalculateVariance(bdCounts);

        // === Summary and Best Method ===
        Console.WriteLine("\n=== Variance Analysis ===");
        foreach (var kv in variances)
        {
            Console.WriteLine($"{kv.Key}: variance = {kv.Value:F2}");
        }

        var bestMethod = variances.OrderBy(kv => kv.Value).First();
        Console.WriteLine($"\nSuggested Best Binning Method: {bestMethod.Key} (lowest variance)");
    }

    // calculate variance
    public static double CalculateVariance(List<int> counts)
    {
        if (counts.Count == 0) return 0;
        double mean = counts.Average();
        double variance = counts.Select(c => Math.Pow(c - mean, 2)).Average();
        return variance;
    }

    // print bin counts
    public static void PrintBinCounts(List<int> counts)
    {
        for (int i = 0; i < counts.Count; i++)
        {
            Console.WriteLine($"Bin {i}: {counts[i]} logs");
        }
    }
}

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
            int binIndex = (int)(minutesFromStart / binWidth);
            if (binIndex >= numberOfBins) binIndex = numberOfBins - 1;
            return binIndex;
        }).OrderBy(b => b.Key).ToList();

        var ewCounts = equalWidthBins.Select(b => b.Count()).ToList();
        PrintBinCounts(ewCounts);
        variances["Equal Width"] = CalculateVariance(ewCounts);
        entropyScores["Equal Width"] = CalculateEntropy(ewCounts);
        balanceRatios["Equal Width"] = CalculateBalanceRatio(ewCounts);

        // === Equal Frequency Binning ===
        Console.WriteLine("\n=== Equal Frequency (Quantile) Binning ===");
        var sortedLogs = logs.OrderBy(log => log.Timestamp).ToList();
        int binSize = logs.Count / numberOfBins;
        var efCounts = new List<int>();

        for (int i = 0; i < numberOfBins; i++)
        {
            var binLogs = (i == numberOfBins - 1)
                ? sortedLogs.Skip(i * binSize).ToList()
                : sortedLogs.Skip(i * binSize).Take(binSize).ToList();

            efCounts.Add(binLogs.Count);
            DateTime binStart = binLogs.First().Timestamp;
            DateTime binEnd = binLogs.Last().Timestamp;
            Console.WriteLine($"Bin {i + 1} [{binStart:HH:mm:ss} - {binEnd:HH:mm:ss}]: {binLogs.Count} logs");
        }
        variances["Equal Frequency"] = CalculateVariance(efCounts);
        entropyScores["Equal Frequency"] = CalculateEntropy(efCounts);
        balanceRatios["Equal Frequency"] = CalculateBalanceRatio(efCounts);

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
        entropyScores["Business Domain"] = CalculateEntropy(bdCounts);
        balanceRatios["Business Domain"] = CalculateBalanceRatio(bdCounts);

        // === Expanded Summary and Best Method ===
        Console.WriteLine("\n=== Expanded Metrics Analysis ===");
        var compositeScores = new Dictionary<string, double>();

        foreach (var method in variances.Keys)
        {
            double variance = variances[method];
            double entropy = entropyScores[method];
            double balanceRatio = balanceRatios[method];

            double compositeScore = variance + entropy + balanceRatio; // simple sum
            compositeScores[method] = compositeScore;

            Console.WriteLine($"{method}: variance = {variance:F2}, entropy = {entropy:F2}, balance ratio = {balanceRatio:F2}, composite score = {compositeScore:F2}");
        }

        var bestComposite = compositeScores.OrderBy(kv => kv.Value).First();
        Console.WriteLine($"\nSuggested Best Binning Method (composite score): {bestComposite.Key}");
    }

    // calculate variance
    public static double CalculateVariance(List<int> counts)
    {
        if (counts.Count == 0) return 0;
        double mean = counts.Average();
        double variance = counts.Select(c => Math.Pow(c - mean, 2)).Average();
        return variance;
    }

    // calculate entropy
    public static double CalculateEntropy(List<int> counts)
    {
        int total = counts.Sum();
        if (total == 0) return 0;

        double entropy = 0.0;
        foreach (var count in counts)
        {
            if (count == 0) continue;
            double p = (double)count / total;
            entropy -= p * Math.Log(p, 2);
        }
        return entropy;
    }

    // calculate balance ratio (max/min)
    public static double CalculateBalanceRatio(List<int> counts)
    {
        if (counts.Count == 0) return 0;
        int max = counts.Max();
        int min = counts.Min() == 0 ? 1 : counts.Min(); // avoid division by zero
        return (double)max / min;
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

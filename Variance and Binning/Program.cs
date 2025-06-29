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

        // ========== 1. Binning by Minute (Original Method) ==========
        Console.WriteLine("=== Binning by Minute ===");
        // Group logs by each minute
        // Example: logs from 12:01:04 and 12:01:07 will be grouped together
        var binnedLogs = logs
            .GroupBy(log => new DateTime(log.Timestamp.Year, log.Timestamp.Month, log.Timestamp.Day, log.Timestamp.Hour, log.Timestamp.Minute, 0))
            .OrderBy(group => group.Key)
            .ToList();

        foreach (var bin in binnedLogs)
        {
            int count = bin.Count();
            Console.WriteLine($"{bin.Key:yyyy-MM-dd HH:mm}: {count} logs");

            // If there are more than 3 logs in a minute, we may have a traffic spike
            if (count > 3)
            {
                Console.WriteLine($"Potential traffic spike at {bin.Key:yyyy-MM-dd HH:mm}");
            }
        }

        // Prepare total time range for other binning techniques
        DateTime minTime = logs.Min(l => l.Timestamp);
        DateTime maxTime = logs.Max(l => l.Timestamp);
        double totalMinutes = (maxTime - minTime).TotalMinutes;

        // ========== 2. Equal Width Binning ==========
        Console.WriteLine("\n=== Equal Width Binning (3 Bins) ===");
        // Divide the total time range into 3 bins of equal length
        int numberOfBins = 3;
        double binWidth = totalMinutes / numberOfBins;

        // Group logs based on how far they are from the start, using equal time windows
        var equalWidthBins = logs.GroupBy(log =>
        {
            double minutesFromStart = (log.Timestamp - minTime).TotalMinutes;
            int binIndex = (int)(minutesFromStart / binWidth); // Which bin this log belongs to
            return binIndex;
        });

        foreach (var bin in equalWidthBins.OrderBy(b => b.Key))
        {
            Console.WriteLine($"Bin {bin.Key}: {bin.Count()} logs");
        }

        // ========== 3. Equal Frequency (Quantile) Binning ==========
        Console.WriteLine("\n=== Equal Frequency (Quantile) Binning ===");
        // Sort logs by time so we can split them evenly
        var sortedLogs = logs.OrderBy(log => log.Timestamp).ToList();
        int binSize = logs.Count / numberOfBins;

        // Divide logs into 3 groups where each group has (about) the same number of logs
        for (int i = 0; i < numberOfBins; i++)
        {
            var binLogs = sortedLogs.Skip(i * binSize).Take(binSize).ToList();
            DateTime binStart = binLogs.First().Timestamp;
            DateTime binEnd = binLogs.Last().Timestamp;

            Console.WriteLine($"Bin {i + 1} [{binStart:HH:mm:ss} - {binEnd:HH:mm:ss}]: {binLogs.Count} logs");
        }

        // ========== 4. Business Domain-Based Binning ==========
        // Example: we divide the time into periods we care about for the business
        // Morning: 12:00 - 12:02, Midday: 12:03 - 12:04, Afternoon: 12:05+
        Console.WriteLine("\n=== Business Domain-Based Binning ===");

        var domainBins = logs.GroupBy(log =>
        {
            if (log.Timestamp.Minute <= 2)
                return "Morning"; // Early part of the time range
            else if (log.Timestamp.Minute <= 4)
                return "Midday"; // Middle part of the time range
            else
                return "Afternoon"; // Later part of the time range
        });

        foreach (var bin in domainBins)
        {
            Console.WriteLine($"{bin.Key}: {bin.Count()} logs");
        }
    }
}

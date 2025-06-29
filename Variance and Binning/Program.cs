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

        // Binning logs by minute
        var binnedLogs = logs
            .GroupBy(log => new DateTime(log.Timestamp.Year, log.Timestamp.Month, log.Timestamp.Day, log.Timestamp.Hour, log.Timestamp.Minute, 0))
            .OrderBy(group => group.Key)
            .ToList();

        Console.WriteLine("Log counts per minute:");
        foreach (var bin in binnedLogs)
        {
            int count = bin.Count();
            Console.WriteLine($"{bin.Key:yyyy-MM-dd HH:mm}: {count} logs");

            // Simple spike detection: flag bins with more than 3 logs
            if (count > 3)
            {
                Console.WriteLine($"Potential traffic spike at {bin.Key:yyyy-MM-dd HH:mm}");
            }
        }
    }
}

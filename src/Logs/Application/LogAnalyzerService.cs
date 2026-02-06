using Logs.Core.Entities;
using Logs.Core.Interfaces;
using Logs.Core.ValueObjects;
using Serilog;

namespace Logs.Application;

public sealed class LogAnalyzerService(IEnumerable<ILogProvider> providers, ILogParser parser)
{
    public async Task<LogStatistics> AnalyzeAsync(IReadOnlyList<string> paths, DateRange dateRange)
    {
        var stats = new LogStatistics();

        foreach (string path in paths)
        {
            ILogProvider? provider = providers.FirstOrDefault(p => p.CanHandle(path));

            if (provider is null)
            {
                throw new FileNotFoundException($"No provider found for {path}");
            }

            Log.Information("Using provider: {ProviderType} for path: {Path}", provider.GetType().Name, path);

            await foreach (string line in provider.GetLines(path))
            {
                LogEntry? entry = parser.Parse(line);
                if (entry is null)
                    continue;

                if (dateRange.IsInRange(entry.TimeLocal))
                {
                    stats.AddEntry(entry);
                }
            }
        }

        return stats;
    }

    public LogStatistics Analyze(List<string> paths, DateRange dateRange)
        => AnalyzeAsync(paths, dateRange).GetAwaiter().GetResult();
}
using Logs.Application;
using Logs.Core.Interfaces;
using Logs.Core.ValueObjects;
using Logs.Infrastructure.FileSystem;
using Logs.Infrastructure.Parsing;
using Logs.Infrastructure.Reporting;
using Serilog;

namespace Logs.Infrastructure.Cli;

public sealed class AnalyzeCommand
{
    private readonly List<string> _paths;
    private readonly string _format;
    private readonly string _output;
    private readonly DateRange _dateRange;

    public AnalyzeCommand(IReadOnlyList<string> paths, string format, string output, DateRange dateRange)
    {
        _paths = paths.ToList();
        _format = format;
        _output = output;
        _dateRange = dateRange;
    }

    public async Task ExecuteAsync()
    {
        var providers = new List<ILogProvider>
        {
            new LocalLogProvider(),
            new RemoteLogProvider(),
        };

        ILogParser parser = new NginxLogParser();
        var service = new LogAnalyzerService(providers, parser);

        var stats = await service.AnalyzeAsync(_paths, _dateRange);

        IReportVisitor visitor = _format.ToLowerInvariant() switch
        {
            "json" => new JsonReportVisitor(),
            "markdown" or "md" => new MarkdownReportVisitor(),
            "adoc" or "asciidoc" => new AdocReportVisitor(),
            _ => new MarkdownReportVisitor(),
        };

        visitor.Visit(stats);

        string report = visitor.GetResult();

        if (!string.IsNullOrWhiteSpace(_output))
        {
            string outPath = NormalizePath(_output);
            File.WriteAllText(outPath, report);
            Log.Information("Report written to: {Path}", outPath);
        }
        else
        {
            Console.WriteLine(report);
        }
    }

    public void Execute()
        => ExecuteAsync().GetAwaiter().GetResult();

    private static string NormalizePath(string path)
    {
        if (path.StartsWith("/mnt/", StringComparison.OrdinalIgnoreCase))
        {
            string trimmed = path.Substring(5);
            string[] parts = trimmed.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 1 && parts[0].Length == 1 && char.IsLetter(parts[0][0]))
            {
                string drive = parts[0].ToUpperInvariant();
                string rest = string.Join(Path.DirectorySeparatorChar, parts.Skip(1));
                return $"{drive}:{Path.DirectorySeparatorChar}{rest}";
            }
        }

        return path;
    }
}

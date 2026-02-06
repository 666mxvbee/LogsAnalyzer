using System.Globalization;
using Logs.Core.ValueObjects;

namespace Logs.Infrastructure.Cli;

public sealed class ArgParser
{

    public AnalyzeCommand Parse(string[] args)
    {
        List<string> paths = new();
        string format = "markdown";
        string? output = null;
        DateTimeOffset? from = null;
        DateTimeOffset? to = null;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg == "--path" || arg == "-p")
            {
                if (i + 1 < args.Length)
                {
                    i++;
                    while (i < args.Length && !args[i].StartsWith("-"))
                    {
                        paths.Add(args[i]);
                        i++;
                    }
                    i--;
                }
            }
            else if (arg == "--format" || arg == "-f")
            {
                if (i + 1 < args.Length)
                {
                    format = args[++i];
                }
                else
                {
                    throw new ArgumentException("Format value is required after --format or -f");
                }
            }
            else if (arg == "--output" || arg == "-o")
            {
                if (i + 1 < args.Length)
                {
                    output = args[++i];
                }
            }
            else if (arg.StartsWith("--from="))
            {
                var dateStr = arg.Substring(7).Trim('"');
                from = ParseDate(dateStr);
            }
            else if (arg == "--from" && i + 1 < args.Length)
            {
                var dateStr = args[++i].Trim('"');
                from = ParseDate(dateStr);
            }
            else if (arg.StartsWith("--to="))
            {
                var dateStr = arg.Substring(5).Trim('"');
                to = ParseDate(dateStr);
            }
            else if (arg == "--to" && i + 1 < args.Length)
            {
                var dateStr = args[++i].Trim('"');
                to = ParseDate(dateStr);
            }
            else if (arg.StartsWith("-") && !arg.StartsWith("--"))
            {
                throw new ArgumentException($"Unsupported parameter: {arg}");
            }
            else if (arg.StartsWith("--") && arg != "--from" && arg != "--to" && arg != "--path" && arg != "--format" && arg != "--output")
            {
                if (arg.Contains("="))
                {
                    var paramName = arg.Split('=')[0];
                    if (paramName != "--from" && paramName != "--to" && paramName != "--path" && paramName != "--format" && paramName != "--output")
                    {
                        throw new ArgumentException($"Unsupported parameter: {paramName}");
                    }
                }
                else
                {
                    throw new ArgumentException($"Unsupported parameter: {arg}");
                }
            }
        }

        if (paths.Count == 0)
        {
            throw new ArgumentException("Path is required (--path or -p)");
        }

        if (string.IsNullOrEmpty(format))
        {
            throw new ArgumentException("Format is required (--format or -f)");
        }

        if (string.IsNullOrEmpty(output))
        {
            throw new ArgumentException("Output is required (--output or -o)");
        }

        if (from.HasValue && to.HasValue && from > to)
        {
            throw new ArgumentException("Date 'from' cannot be later than 'to'");
        }

        return new AnalyzeCommand(paths, format, output, new DateRange(from, to));
    }

    private DateTimeOffset ParseDate(string dateStr)
    {
        return DateTimeOffset.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : throw new ArgumentException($"Invalid date format: {dateStr}");
    }
}
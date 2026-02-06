using System.Globalization;
using Logs.Core.Entities;
using Logs.Core.Interfaces;
using Serilog;

namespace Logs.Infrastructure.Parsing;

public sealed class NginxLogParser : ILogParser
{
    public LogEntry? Parse(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        try
        {
            var span = line.AsSpan();

            var ipEnd = span.IndexOf(' ');
            var remoteAddress = span[..ipEnd].ToString();

            var dateStart = span.IndexOf('[') + 1;
            var dateEnd = span.IndexOf(']');
            var dateRaw = span.Slice(dateStart, dateEnd - dateStart);

            if (!DateTimeOffset.TryParseExact(dateRaw, "dd/MMM/yyyy:HH:mm:ss zzz",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var timeLocal) &&
                !DateTimeOffset.TryParseExact(dateRaw, "d/MMM/yyyy:HH:mm:ss zzz",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out timeLocal))
            {
                Log.Warning("Failed to parse date in line: {line}", line);
                return null;
            }

            var requestStart = span.IndexOf('"') + 1;
            var requestEnd = span[requestStart..].IndexOf('"') + requestStart;
            var requestRaw = span.Slice(requestStart, requestEnd - requestStart).ToString();

            var requestParts = requestRaw.Split(' ');
            var method = requestParts.Length > 0 ? requestParts[0] : "-";
            var resource = requestParts.Length > 1 ? requestParts[1] : "-";
            var protocol = requestParts.Length > 2 ? requestParts[2] : "-";

            var restSpan = span[(requestEnd + 2)..];
            var nextSpace = restSpan.IndexOf(' ');
            var statusStr = restSpan[..nextSpace];

            var bytesStart = nextSpace + 1;
            var bytesEnd = restSpan[bytesStart..].IndexOf(' ') + bytesStart;
            var bytesStr = restSpan.Slice(bytesStart, bytesEnd - bytesStart);

            if (!int.TryParse(statusStr, out var statusCode))
            {
                statusCode = 0;
            }

            if (!long.TryParse(bytesStr, out var bodyBytes))
            {
                bodyBytes = 0;
            }

            var lastQuote = line.LastIndexOf('"');
            var preLastQuote = line.LastIndexOf('"', lastQuote - 1);
            var userAgent = line.Substring(preLastQuote + 1, lastQuote - preLastQuote - 1);

            return new LogEntry(remoteAddress, null, timeLocal, resource, method, protocol, statusCode, bodyBytes, "-", userAgent);
        }
        catch (Exception)
        {
            Log.Warning("Skipping malformed line: {line}", line);
            return null;
        }
    }
}
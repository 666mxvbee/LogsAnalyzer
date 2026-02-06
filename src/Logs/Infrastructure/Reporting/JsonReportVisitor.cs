using System.Text.Json;
using Logs.Core.Entities;
using Logs.Core.Interfaces;

namespace Logs.Infrastructure.Reporting;

public sealed class JsonReportVisitor : IReportVisitor
{
    private string _result = string.Empty;

    public void Visit(LogStatistics statistics)
    {
        var report = statistics.Calculate();
        
        var files = report.FileNames.Select(System.IO.Path.GetFileName)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var resources = report.TopResources
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Key, StringComparer.Ordinal)
            .Select(kv => new
            {
                resource = kv.Key,
                totalRequestsCount = kv.Value
            })
            .ToArray();

        var responseCodes = report.ResponseCodes
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Key)
            .Select(kv => new
            {
                code = kv.Key,
                totalResponsesCount = kv.Value
            })
            .ToArray();

        var requestsPerDate = report.RequestsPerDate
            .OrderBy(x => x.Key)
            .Select(kv => new
            {
                date = kv.Key.ToString("yyyy-MM-dd"),
                weekday = kv.Key.DayOfWeek.ToString(),
                totalRequestsCount = kv.Value,
                totalRequestsPercentage = report.TotalRequests > 0
                    ? Math.Round((double)kv.Value / report.TotalRequests * 100, 2)
                    : 0
            })
            .ToArray();

        var uniqueProtocols = report.UniqueProtocols
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

        var output = new
        {
            files = files,
            totalRequestsCount = report.TotalRequests,
            responseSizeInBytes = new
            {
                average = Math.Round(report.AverageResponseSize, 2),
                max = report.MaxResponseSize,
                p95 = report.P95ResponseSize
            },
            resources = resources,
            responseCodes = responseCodes,
            requestsPerDate = requestsPerDate,
            uniqueProtocols = uniqueProtocols
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        _result = JsonSerializer.Serialize(output, options);
    }

    public string GetResult()
        => _result;

    public string GetExtension()
        => ".json";
}
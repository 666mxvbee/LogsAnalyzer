namespace Logs.Core.ValueObjects;

public record StatisticReport(
    IReadOnlyList<string> FileNames,
    long TotalRequests,
    long TotalBytes,
    double AverageResponseSize,
    long MaxResponseSize,
    long P95ResponseSize,
    IReadOnlyDictionary<string, int> TopResources,
    IReadOnlyDictionary<int, int> ResponseCodes,
    IReadOnlyDictionary<DateOnly, int> RequestsPerDate,
    IReadOnlyList<string> UniqueProtocols);
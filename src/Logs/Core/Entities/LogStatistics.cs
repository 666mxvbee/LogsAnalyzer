using Logs.Core.Interfaces;
using Logs.Core.ValueObjects;

namespace Logs.Core.Entities;

public sealed class LogStatistics
{
    public List<string> FileNames { get; } = [];

    private long _totalRequests;
    private long _totalBytes;
    private long _maxResponseSize;
    private readonly List<long> _responseSizes = [];
    private readonly Dictionary<string, int> _resources = [];
    private readonly Dictionary<int, int> _responseCodes = [];
    private readonly Dictionary<DateOnly, int> _requestsPerDate = [];
    private readonly HashSet<string> _uniqueProtocols = [];

    public void AddFile(string fileName)
    {
        FileNames.Add(fileName);
    }

    public void AddEntry(LogEntry entry)
    {
        _totalRequests++;
        _totalBytes += entry.BodyBytesSent;

        if (entry.BodyBytesSent > _maxResponseSize)
        {
            _maxResponseSize = entry.BodyBytesSent;
        }
        
        _responseSizes.Add(entry.BodyBytesSent);

        if (!_resources.TryAdd(entry.RequestResource, 1))
        {
            _resources[entry.RequestResource]++;
        }

        if (!_responseCodes.TryAdd(entry.StatusCode, 1))
        {
            _responseCodes[entry.StatusCode]++;
        }

        var dateKey = DateOnly.FromDateTime(entry.TimeLocal.DateTime);
        if (!_requestsPerDate.TryAdd(dateKey, 1))
        {
            _requestsPerDate[dateKey]++;
        }

        _uniqueProtocols.Add(entry.Protocol);
    }

    public StatisticReport Calculate()
    {
        _responseSizes.Sort();

        double avg = 0;
        long p95 = 0;

        if (_responseSizes.Count > 0)
        {
            avg = _responseSizes.Average();
            var p95Index = (int)Math.Ceiling(_responseSizes.Count * 0.95) - 1;

            if (p95Index < 0)
            {
                p95Index = 0;
            }

            if (p95Index >= _responseSizes.Count)
            {
                p95Index = _responseSizes.Count - 1;
            }
            
            p95 = _responseSizes[p95Index];
        }
        
        var topResources = _resources
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToDictionary(x => x.Key, x => x.Value);

        return new StatisticReport(
            FileNames.AsReadOnly(),
            _totalRequests,
            _totalBytes,
            avg,
            _maxResponseSize,
            p95,
            topResources,
            _responseCodes.AsReadOnly(),
            _requestsPerDate.AsReadOnly(),
            _uniqueProtocols.ToList().AsReadOnly());
    }

    public void Accept(IReportVisitor visitor)
    {
        visitor.Visit(this);
    }
}
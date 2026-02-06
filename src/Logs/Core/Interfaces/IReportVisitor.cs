using Logs.Core.Entities;

namespace Logs.Core.Interfaces;

public interface IReportVisitor
{
    void Visit(LogStatistics statistics);
    string GetResult();
    string GetExtension();
}
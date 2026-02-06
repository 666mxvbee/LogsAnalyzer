using Logs.Core.Entities;

namespace Logs.Core.Interfaces;

public interface ILogParser
{
    LogEntry? Parse(string line);
}
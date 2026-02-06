namespace Logs.Core.Interfaces;

public interface ILogProvider
{
    IAsyncEnumerable<string> GetLines(string path);
    bool CanHandle(string path);
}
using Logs.Core.Interfaces;

namespace Logs.Infrastructure.FileSystem;

public sealed class RemoteLogProvider : ILogProvider
{
    private static readonly HttpClient Http = new();

    public bool CanHandle(string path)
        => Uri.TryCreate(path, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    public async IAsyncEnumerable<string> GetLines(string path)
    {
        using HttpResponseMessage response = await Http.GetAsync(
            path,
            HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (true)
        {
            string? line = await reader.ReadLineAsync();
            if (line is null) yield break;
            yield return line;
        }
    }
}
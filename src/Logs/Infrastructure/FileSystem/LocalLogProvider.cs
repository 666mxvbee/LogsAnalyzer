using Logs.Core.Interfaces;

namespace Logs.Infrastructure.FileSystem;

public sealed class LocalLogProvider : ILogProvider
{
    public bool CanHandle(string path)
        => !path.StartsWith("http", StringComparison.OrdinalIgnoreCase);

    public async IAsyncEnumerable<string> GetLines(string path)
    {
        path = NormalizePath(path);

        string? directory = Path.GetDirectoryName(path);
        string pattern = Path.GetFileName(path);

        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("File name or pattern is required");
        }

        if (string.IsNullOrWhiteSpace(directory))
        {
            directory = Directory.GetCurrentDirectory();
        }

        List<string> files = Directory.EnumerateFiles(directory, pattern).ToList();

        if (files.Count == 0)
        {
            throw new FileNotFoundException($"No files found by pattern '{pattern}' in '{directory}'");
        }

        foreach (string file in files)
        {
            await foreach (string line in ReadFileLinesAsync(file))
            {
                yield return line;
            }
        }
    }

    private static async IAsyncEnumerable<string> ReadFileLinesAsync(string file)
    {
        await using var fs = new FileStream(
            file,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        using var reader = new StreamReader(fs);

        while (!reader.EndOfStream)
        {
            string? line = await reader.ReadLineAsync();
            if (line is not null)
                yield return line;
        }
    }

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

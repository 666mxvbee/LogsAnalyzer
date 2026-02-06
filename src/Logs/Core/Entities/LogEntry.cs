namespace Logs.Core.Entities;

public record LogEntry(
    string RemoteAddress,
    string? RemoteUser,
    DateTimeOffset TimeLocal,
    string RequestResource,
    string RequestMethod,
    string Protocol,
    int StatusCode,
    long BodyBytesSent,
    string HttpReferer,
    string HttpUserAgent);
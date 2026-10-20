using System;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class QueryComment
{
    private QueryComment(string text, bool compact)
    {
        Text = text;
        Compact = compact;
    }

    public string Text { get; }
    public bool Compact { get; }

    public static QueryComment Create(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        var compact = text.Length <= 120 && text.IndexOfAny(new[] { '\r', '\n' }) < 0;
        return new QueryComment(text, compact);
    }
}
using System;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class QueryCommentFormatter
{
    public static string CreateCompactToken(string text)
        => "/* " + EscapeDelimitedText(text) + " */";

    public static string FormatDisplayText(string text)
        => text.Normalize(NormalizationForm.FormKC).ReplaceLineEndings(Environment.NewLine);

    private static string EscapeDelimitedText(string text)
        => text.Replace("/*", "/ *", StringComparison.Ordinal)
            .Replace("*/", "* /", StringComparison.Ordinal);
}
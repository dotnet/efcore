using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class QueryCommentWriter
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    public QueryCommentWriter(ISqlGenerationHelper sqlGenerationHelper)
        => _sqlGenerationHelper = sqlGenerationHelper;

    public string GenerateHeader(IReadOnlyList<QueryComment> comments)
    {
        var builder = new StringBuilder();
        foreach (var comment in comments)
        {
            if (comment.Compact)
            {
                var token = QueryCommentFormatter.CreateCompactToken(comment.Text);
                builder.AppendLine(QueryCommentFormatter.FormatDisplayText(token));
            }
            else
            {
                var displayText = QueryCommentFormatter.FormatDisplayText(comment.Text);
                builder.Append(_sqlGenerationHelper.GenerateComment(displayText));
            }
        }

        return builder.ToString();
    }
}
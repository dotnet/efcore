using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query;

public partial class QuerySqlGenerator
{
    public virtual IRelationalCommand GetCommand(SelectExpression selectExpression)
    {
        _relationalCommandBuilder = _relationalCommandBuilderFactory.Create();
        GenerateTagsHeaderComment(selectExpression.Tags);
        var writer = new QueryCommentWriter(_sqlGenerationHelper);
        _relationalCommandBuilder.Append(writer.GenerateHeader(selectExpression.Comments));
        VisitSelect(selectExpression);
        return _relationalCommandBuilder.Build();
    }

    protected virtual void GenerateTagsHeaderComment(ISet<string> tags)
    {
        foreach (var tag in tags)
        {
            _relationalCommandBuilder.AppendLines(_sqlGenerationHelper.GenerateComment(tag));
        }

        if (tags.Count > 0)
        {
            _relationalCommandBuilder.AppendLine();
        }
    }
}
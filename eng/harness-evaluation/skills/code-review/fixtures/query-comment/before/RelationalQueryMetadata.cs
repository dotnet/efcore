namespace Microsoft.EntityFrameworkCore.Query;

public partial class RelationalQueryableMethodTranslatingExpressionVisitor
{
    private void ApplyQueryMetadata(SelectExpression selectExpression)
        => selectExpression.ApplyTags(QueryCompilationContext.Tags);
}

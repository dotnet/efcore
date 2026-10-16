using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class QueryCompilationContext
    {
        public virtual ISet<string> Tags { get; } = new HashSet<string>();

        public virtual void AddTag(string tag)
            => Tags.Add(tag);
    }
}

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public partial class QueryableMethodNormalizingExpressionVisitor
    {
        private Expression? ExtractQueryMetadata(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.Name == nameof(EntityFrameworkQueryableExtensions.TagWith))
            {
                var source = Visit(methodCallExpression.Arguments[0]);
                _queryCompilationContext.AddTag((string)((ConstantExpression)methodCallExpression.Arguments[1]).Value!);
                return source;
            }

            return null;
        }
    }
}
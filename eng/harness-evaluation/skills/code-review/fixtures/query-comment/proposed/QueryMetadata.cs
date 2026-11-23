using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class QueryCompilationContext
    {
        private readonly List<QueryComment> _comments = new();

        public virtual ISet<string> Tags { get; } = new HashSet<string>();

        public virtual void AddTag(string tag)
            => Tags.Add(tag);

        [EntityFrameworkInternal]
        public virtual IReadOnlyList<QueryComment> Comments => _comments;

        [EntityFrameworkInternal]
        public virtual void AddComment(string comment)
            => _comments.Add(QueryComment.Create(comment));
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

            if (methodCallExpression.Method.Name == nameof(EntityFrameworkQueryableExtensions.Comment))
            {
                var source = Visit(methodCallExpression.Arguments[0]);
                _queryCompilationContext.AddComment((string)((ConstantExpression)methodCallExpression.Arguments[1]).Value!);
                return source;
            }

            return null;
        }
    }
}
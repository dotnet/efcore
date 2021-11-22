// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class
    ComplexNavigationsCollectionsSplitQueryRelationalTestBase<TFixture> : ComplexNavigationsCollectionsQueryTestBase<TFixture>
    where TFixture : ComplexNavigationsQueryFixtureBase, new()
{
    protected ComplexNavigationsCollectionsSplitQueryRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
        => new SplitQueryRewritingExpressionVisitor().Visit(serverQueryExpression);

    private class SplitQueryRewritingExpressionVisitor : ExpressionVisitor
    {
        private readonly MethodInfo _asSplitQueryMethod
            = typeof(RelationalQueryableExtensions).GetMethod(nameof(RelationalQueryableExtensions.AsSplitQuery));

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is QueryRootExpression rootExpression)
            {
                var splitMethod = _asSplitQueryMethod.MakeGenericMethod(rootExpression.EntityType.ClrType);

                return Expression.Call(splitMethod, rootExpression);
            }

            return base.VisitExtension(extensionExpression);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ComplexNavigationsCollectionsSplitSharedQueryTypeRelationalTestBase<TFixture> : ComplexNavigationsCollectionsSharedTypeQueryTestBase<TFixture>
        where TFixture : ComplexNavigationsSharedTypeQueryRelationalFixtureBase, new()
    {
        protected ComplexNavigationsCollectionsSplitSharedQueryTypeRelationalTestBase(TFixture fixture)
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
}

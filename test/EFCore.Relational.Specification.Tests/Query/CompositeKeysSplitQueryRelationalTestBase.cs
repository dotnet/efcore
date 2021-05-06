// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class CompositeKeysSplitQueryRelationalTestBase<TFixture> : CompositeKeysQueryTestBase<TFixture>
        where TFixture : CompositeKeysQueryFixtureBase, new()
    {
        public CompositeKeysSplitQueryRelationalTestBase(TFixture fixture)
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

        protected virtual bool CanExecuteQueryString
            => false;

        protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
            => new RelationalQueryAsserter(
                fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression, canExecuteQueryString: CanExecuteQueryString);
    }
}

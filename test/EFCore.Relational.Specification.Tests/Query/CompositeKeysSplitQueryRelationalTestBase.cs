﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class CompositeKeysSplitQueryRelationalTestBase<TFixture>(TFixture fixture) : CompositeKeysQueryTestBase<TFixture>(fixture)
    where TFixture : CompositeKeysQueryFixtureBase, new()
{
    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
    {
        serverQueryExpression = base.RewriteServerQueryExpression(serverQueryExpression);

        return new SplitQueryRewritingExpressionVisitor().Visit(serverQueryExpression);
    }

    private class SplitQueryRewritingExpressionVisitor : ExpressionVisitor
    {
        private readonly MethodInfo _asSplitQueryMethod
            = typeof(RelationalQueryableExtensions).GetMethod(nameof(RelationalQueryableExtensions.AsSplitQuery));

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is EntityQueryRootExpression rootExpression)
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

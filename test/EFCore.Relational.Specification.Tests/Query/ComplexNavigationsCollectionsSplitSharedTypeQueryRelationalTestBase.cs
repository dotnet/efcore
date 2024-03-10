// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class
    ComplexNavigationsCollectionsSplitSharedTypeQueryRelationalTestBase<TFixture> : ComplexNavigationsCollectionsSharedTypeQueryTestBase
        <TFixture>
    where TFixture : ComplexNavigationsSharedTypeQueryRelationalFixtureBase, new()
{
    protected ComplexNavigationsCollectionsSplitSharedTypeQueryRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    public override async Task SelectMany_with_navigation_and_Distinct_projecting_columns_including_join_key(bool async)
        => Assert.Equal(
            RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_with_navigation_and_Distinct_projecting_columns_including_join_key(async))).Message);

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
}

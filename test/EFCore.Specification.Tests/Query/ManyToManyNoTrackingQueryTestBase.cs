// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class ManyToManyNoTrackingQueryTestBase<TFixture> : ManyToManyQueryTestBase<TFixture>
    where TFixture : ManyToManyQueryFixtureBase, new()
{
    private static readonly MethodInfo _asNoTrackingMethodInfo
        = typeof(EntityFrameworkQueryableExtensions)
            .GetTypeInfo().GetDeclaredMethod(nameof(EntityFrameworkQueryableExtensions.AsNoTracking));

    protected ManyToManyNoTrackingQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    protected override bool IgnoreEntryCount
        => true;

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
    {
        serverQueryExpression = base.RewriteServerQueryExpression(serverQueryExpression);

        var elementType = serverQueryExpression.Type.TryGetSequenceType();

        if (elementType.UnwrapNullableType().IsValueType
            && serverQueryExpression is MethodCallExpression methodCallExpression
            && methodCallExpression.Method.DeclaringType == typeof(Queryable))
        {
            return methodCallExpression.Update(
                null, new[] { ApplyNoTracking(methodCallExpression.Arguments[0]) }
                    .Concat(methodCallExpression.Arguments.Skip(1)));
        }

        return ApplyNoTracking(serverQueryExpression);

        static Expression ApplyNoTracking(Expression source)
            => Expression.Call(
                _asNoTrackingMethodInfo.MakeGenericMethod(source.Type.TryGetSequenceType()),
                source);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include_skip_navigation_then_include_inverse_throws_in_no_tracking(bool async)
        => Assert.Equal(
            CoreStrings.IncludeWithCycle(nameof(EntityThree.OneSkipPayloadFullShared), nameof(EntityOne.ThreeSkipPayloadFullShared)),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<EntityThree>().AsNoTracking().Include(e => e.OneSkipPayloadFullShared)
                        .ThenInclude(e => e.ThreeSkipPayloadFullShared),
                    elementAsserter: (e, a) => AssertInclude(
                        e, a,
                        new ExpectedInclude<EntityThree>(et => et.OneSkipPayloadFullShared),
                        new ExpectedInclude<EntityOne>(et => et.ThreeSkipPayloadFullShared, "OneSkipPayloadFullShared"))))).Message);

    public override Task Include_skip_navigation_then_include_inverse_works_for_tracking_query(bool async)
        => Task.CompletedTask;

    public override Task Include_skip_navigation_then_include_inverse_works_for_tracking_query_unidirectional(bool async)
        => Task.CompletedTask;
}

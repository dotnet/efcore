// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
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
            {
                return Expression.Call(
                    _asNoTrackingMethodInfo.MakeGenericMethod(source.Type.TryGetSequenceType()),
                    source);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_skip_navigation_then_include_inverse_throws_in_no_tracking(bool async)
        {
            Assert.Equal(
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
        }

        public override Task Include_skip_navigation_then_include_inverse_works_for_tracking_query(bool async)
            => Task.CompletedTask;
    }
}

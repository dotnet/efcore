// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using ReLinq = Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ReLinqEvaluatableExpressionFilter : ReLinq.EvaluatableExpressionFilterBase
    {
        // This methods are non-deterministic and result varies based on time of running the query.
        // Hence we don't evaluate them. See issue#2069

        private static readonly PropertyInfo _dateTimeNow
            = typeof(DateTime).GetTypeInfo().GetDeclaredProperty(nameof(DateTime.Now));

        private static readonly PropertyInfo _dateTimeUtcNow
            = typeof(DateTime).GetTypeInfo().GetDeclaredProperty(nameof(DateTime.UtcNow));

        private static readonly PropertyInfo _dateTimeToday
            = typeof(DateTime).GetTypeInfo().GetDeclaredProperty(nameof(DateTime.Today));

        private static readonly PropertyInfo _dateTimeOffsetNow
            = typeof(DateTimeOffset).GetTypeInfo().GetDeclaredProperty(nameof(DateTimeOffset.Now));

        private static readonly PropertyInfo _dateTimeOffsetUtcNow
            = typeof(DateTimeOffset).GetTypeInfo().GetDeclaredProperty(nameof(DateTimeOffset.UtcNow));

        private static readonly MethodInfo _guidNewGuid
            = typeof(Guid).GetRuntimeMethod(nameof(Guid.NewGuid), Type.EmptyTypes);

        private static readonly MethodInfo _randomNextNoArgs
            = typeof(Random).GetRuntimeMethod(nameof(Random.Next), Type.EmptyTypes);

        private static readonly MethodInfo _randomNextOneArg
            = typeof(Random).GetRuntimeMethod(nameof(Random.Next), new[] { typeof(int) });

        private static readonly MethodInfo _randomNextTwoArgs
            = typeof(Random).GetRuntimeMethod(nameof(Random.Next), new[] { typeof(int), typeof(int) });

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool IsEvaluatableMethodCall(MethodCallExpression methodCallExpression)
        {
            var method = methodCallExpression.Method;

            return Equals(method, _guidNewGuid)
                   || Equals(method, _randomNextNoArgs)
                   || Equals(method, _randomNextOneArg)
                   || Equals(method, _randomNextTwoArgs)
                ? false
                : base.IsEvaluatableMethodCall(methodCallExpression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool IsEvaluatableMember(MemberExpression memberExpression)
        {
            var member = memberExpression.Member;

            return Equals(member, _dateTimeNow)
                   || Equals(member, _dateTimeUtcNow)
                   || Equals(member, _dateTimeToday)
                   || Equals(member, _dateTimeOffsetNow)
                   || Equals(member, _dateTimeOffsetUtcNow)
                ? false
                : base.IsEvaluatableMember(memberExpression);
        }
    }
}

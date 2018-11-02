// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{

    public class EvaluatableExpressionFilter : EvaluatableExpressionFilterBase
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
            = typeof(Guid).GetTypeInfo().GetDeclaredMethod(nameof(Guid.NewGuid));

        private static readonly MethodInfo _randomNextNoArgs
            = typeof(Random).GetRuntimeMethod(nameof(Random.Next), Array.Empty<Type>());

        private static readonly MethodInfo _randomNextOneArg
            = typeof(Random).GetRuntimeMethod(nameof(Random.Next), new[] { typeof(int) });

        private static readonly MethodInfo _randomNextTwoArgs
            = typeof(Random).GetRuntimeMethod(nameof(Random.Next), new[] { typeof(int), typeof(int) });

        public override bool IsEvaluatableExpression(Expression expression)
        {
            switch (expression)
            {
                case MemberExpression memberExpression:
                    var member = memberExpression.Member;
                    if (Equals(member, _dateTimeNow)
                        || Equals(member, _dateTimeUtcNow)
                        || Equals(member, _dateTimeToday)
                        || Equals(member, _dateTimeOffsetNow)
                        || Equals(member, _dateTimeOffsetUtcNow))
                    {
                        return false;
                    }
                    break;

                case MethodCallExpression methodCallExpression:
                    var method = methodCallExpression.Method;

                    if (Equals(method, _guidNewGuid)
                        || Equals(method, _randomNextNoArgs)
                        || Equals(method, _randomNextOneArg)
                        || Equals(method, _randomNextTwoArgs))
                    {
                        return false;
                    }
                    break;
            }

            return base.IsEvaluatableExpression(expression);
        }
    }
}

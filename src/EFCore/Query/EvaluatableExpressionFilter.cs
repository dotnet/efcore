// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class EvaluatableExpressionFilter : IEvaluatableExpressionFilter
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

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual EvaluatableExpressionFilterDependencies Dependencies { get; }

        /// <summary>
        ///     <para>
        ///         Creates a new <see cref="EvaluatableExpressionFilter"/> instance.
        ///     </para>
        ///     <para>
        ///         This type is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="dependencies"> The dependencies to use. </param>
        public EvaluatableExpressionFilter(
            [NotNull] EvaluatableExpressionFilterDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsEvaluatableExpression(Expression expression)
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

            return true;
        }
    }
}

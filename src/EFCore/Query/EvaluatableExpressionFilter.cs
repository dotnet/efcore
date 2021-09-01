// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Represents a filter for evaluatable expressions.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     and <see href="https://aka.ms/efcore-how-queries-work">How EF Core queries work</see> for more information.
    /// </remarks>
    public class EvaluatableExpressionFilter : IEvaluatableExpressionFilter
    {
        // This methods are non-deterministic and result varies based on time of running the query.
        // Hence we don't evaluate them. See issue#2069

        private static readonly PropertyInfo _dateTimeNow
            = typeof(DateTime).GetRequiredDeclaredProperty(nameof(DateTime.Now));

        private static readonly PropertyInfo _dateTimeUtcNow
            = typeof(DateTime).GetRequiredDeclaredProperty(nameof(DateTime.UtcNow));

        private static readonly PropertyInfo _dateTimeToday
            = typeof(DateTime).GetRequiredDeclaredProperty(nameof(DateTime.Today));

        private static readonly PropertyInfo _dateTimeOffsetNow
            = typeof(DateTimeOffset).GetRequiredDeclaredProperty(nameof(DateTimeOffset.Now));

        private static readonly PropertyInfo _dateTimeOffsetUtcNow
            = typeof(DateTimeOffset).GetRequiredDeclaredProperty(nameof(DateTimeOffset.UtcNow));

        private static readonly MethodInfo _guidNewGuid
            = typeof(Guid).GetRequiredDeclaredMethod(nameof(Guid.NewGuid));

        private static readonly MethodInfo _randomNextNoArgs
            = typeof(Random).GetRequiredRuntimeMethod(nameof(Random.Next), Array.Empty<Type>());

        private static readonly MethodInfo _randomNextOneArg
            = typeof(Random).GetRequiredRuntimeMethod(nameof(Random.Next), new[] { typeof(int) });

        private static readonly MethodInfo _randomNextTwoArgs
            = typeof(Random).GetRequiredRuntimeMethod(nameof(Random.Next), new[] { typeof(int), typeof(int) });

        /// <summary>
        ///     <para>
        ///         Creates a new <see cref="EvaluatableExpressionFilter" /> instance.
        ///     </para>
        ///     <para>
        ///         This type is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="dependencies"> The dependencies to use. </param>
        public EvaluatableExpressionFilter(
            EvaluatableExpressionFilterDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies for this service.
        /// </summary>
        protected virtual EvaluatableExpressionFilterDependencies Dependencies { get; }

        /// <summary>
        ///     Checks whether the given expression can be evaluated.
        /// </summary>
        /// <param name="expression"> The expression. </param>
        /// <param name="model"> The model. </param>
        /// <returns> <see langword="true" /> if the expression can be evaluated; <see langword="false" /> otherwise. </returns>
        public virtual bool IsEvaluatableExpression(Expression expression, IModel model)
        {
            Check.NotNull(expression, nameof(expression));
            Check.NotNull(model, nameof(model));

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
                        || Equals(method, _randomNextTwoArgs)
                        || method.DeclaringType == typeof(DbFunctionsExtensions))
                    {
                        return false;
                    }

                    break;
            }

            foreach (var plugin in Dependencies.Plugins)
            {
                if (!plugin.IsEvaluatableExpression(expression))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

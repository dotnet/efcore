// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class TaskBlockingExpressionVisitor : ExpressionVisitorBase, ITaskBlockingExpressionVisitor
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Expression Visit(Expression expression)
        {
            if (expression != null)
            {
                var typeInfo = expression.Type.GetTypeInfo();

                if (typeInfo.IsGenericType
                    && typeInfo.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    return Expression.Call(
                        ResultMethodInfo.MakeGenericMethod(typeInfo.GenericTypeArguments[0]),
                        expression);
                }
            }

            return expression;
        }

        internal static MethodInfo ResultMethodInfo { get; }
            = typeof(TaskBlockingExpressionVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(Result));

        [UsedImplicitly]
        private static T Result<T>(Task<T> task) => task.GetAwaiter().GetResult();
    }
}

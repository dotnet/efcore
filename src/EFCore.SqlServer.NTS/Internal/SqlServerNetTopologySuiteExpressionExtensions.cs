// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GeoAPI.Geometries;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class SqlServerNetTopologySuiteExpressionExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static string FindSpatialStoreType([NotNull] this Expression expression)
            => expression.FindProperties()
                .FirstOrDefault(p => typeof(IGeometry).IsAssignableFrom(p.ClrType))
                ?.FindRelationalMapping()
                ?.StoreType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        private static IEnumerable<IProperty> FindProperties([NotNull] this Expression expression)
        {
            switch (expression)
            {
                case ColumnExpression columnExpression:
                    yield return columnExpression.Property;
                    break;

                case UnaryExpression unaryExpression:
                    foreach (var property in unaryExpression.Operand.FindProperties())
                    {
                        yield return property;
                    }

                    break;

                case SqlFunctionExpression functionExpression:
                {
                    IEnumerable<Expression> arguments = functionExpression.Arguments;
                    if (functionExpression.Instance != null)
                    {
                        arguments = arguments.Concat(new[] { functionExpression.Instance });
                    }

                    foreach (var property in arguments.SelectMany(FindProperties))
                    {
                        yield return property;
                    }
                }
                    break;

                case MethodCallExpression methodCallExpression:
                {
                    IEnumerable<Expression> arguments = methodCallExpression.Arguments;
                    if (methodCallExpression.Object != null)
                    {
                        arguments = arguments.Concat(new[] { methodCallExpression.Object });
                    }

                    foreach (var property in arguments.SelectMany(FindProperties))
                    {
                        yield return property;
                    }
                }
                    break;

                case MemberExpression memberExpression:
                    foreach (var property in memberExpression.Expression.FindProperties())
                    {
                        yield return property;
                    }

                    break;
            }
        }
    }
}

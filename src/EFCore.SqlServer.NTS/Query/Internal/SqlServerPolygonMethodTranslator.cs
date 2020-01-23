// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerPolygonMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _getInteriorRingN = typeof(Polygon).GetRuntimeMethod(
            nameof(Polygon.GetInteriorRingN), new[] { typeof(int) });

        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerPolygonMethodTranslator(
            [NotNull] IRelationalTypeMappingSource typeMappingSource,
            [NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _typeMappingSource = typeMappingSource;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));

            if (Equals(method, _getInteriorRingN))
            {
                var storeType = instance.TypeMapping.StoreType;
                var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

                if (isGeography)
                {
                    return _sqlExpressionFactory.Function(
                        instance,
                        "RingN",
                        new[]
                        {
                            _sqlExpressionFactory.Add(
                                arguments[0],
                                _sqlExpressionFactory.Constant(2))
                        },
                        nullResultAllowed: true,
                        instancePropagatesNullability: true,
                        argumentsPropagateNullability: new[] { true },
                        method.ReturnType,
                        _typeMappingSource.FindMapping(method.ReturnType, storeType));
                }

                return _sqlExpressionFactory.Function(
                    instance,
                    "STInteriorRingN",
                    new[]
                    {
                        _sqlExpressionFactory.Add(
                            arguments[0],
                            _sqlExpressionFactory.Constant(1))
                    },
                    nullResultAllowed: true,
                    instancePropagatesNullability: true,
                    argumentsPropagateNullability: new[] { true },
                    method.ReturnType,
                    _typeMappingSource.FindMapping(method.ReturnType, storeType));
            }

            return null;
        }
    }
}

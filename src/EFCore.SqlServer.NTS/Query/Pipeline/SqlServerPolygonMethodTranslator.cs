// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerPolygonMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _getInteriorRingN = typeof(IPolygon).GetRuntimeMethod(nameof(IPolygon.GetInteriorRingN), new[] { typeof(int) });

        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ITypeMappingApplyingExpressionVisitor _typeMappingApplyingExpressionVisitor;

        public SqlServerPolygonMethodTranslator(
            IRelationalTypeMappingSource typeMappingSource,
            ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor)
        {
            _typeMappingSource = typeMappingSource;
            _typeMappingApplyingExpressionVisitor = typeMappingApplyingExpressionVisitor;
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            if (typeof(IPolygon).IsAssignableFrom(method.DeclaringType))
            {
                var storeType = instance.TypeMapping.StoreType;
                var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

                method = method.OnInterface(typeof(IPolygon));
                if (isGeography
                    && Equals(method, _getInteriorRingN))
                {
                    return new SqlFunctionExpression(
                        instance,
                        "RingN",
                        new[] {
                            _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(
                                new SqlBinaryExpression(
                                    ExpressionType.Add,
                                    arguments[0],
                                    new SqlConstantExpression(Expression.Constant(2), null),
                                    typeof(int),
                                    null),
                                _typeMappingSource.FindMapping(typeof(int)))
                        },
                        method.ReturnType,
                        _typeMappingSource.FindMapping(method.ReturnType, storeType),
                        false);
                }
                else if (Equals(method, _getInteriorRingN))
                {
                    return new SqlFunctionExpression(
                        instance,
                        "STInteriorRingN",
                        new[] {
                            _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(
                                new SqlBinaryExpression(
                                    ExpressionType.Add,
                                    arguments[0],
                                    new SqlConstantExpression(Expression.Constant(1), null),
                                    typeof(int),
                                    null),
                                _typeMappingSource.FindMapping(typeof(int)))
                        },
                        method.ReturnType,
                        _typeMappingSource.FindMapping(method.ReturnType, storeType),
                        false);
                }
            }

            return null;
        }
    }
}

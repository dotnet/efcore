// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerGeometryCollectionMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _item = typeof(IGeometryCollection).GetRuntimeProperty("Item").GetMethod;
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ITypeMappingApplyingExpressionVisitor _typeMappingApplyingExpressionVisitor;

        public SqlServerGeometryCollectionMethodTranslator(IRelationalTypeMappingSource typeMappingSource, ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor)
        {
            _typeMappingSource = typeMappingSource;
            _typeMappingApplyingExpressionVisitor = typeMappingApplyingExpressionVisitor;
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            if (typeof(IGeometryCollection).IsAssignableFrom(method.DeclaringType)
                && Equals(method.OnInterface(typeof(IGeometryCollection)), _item))
            {
                return new SqlFunctionExpression(
                    instance,
                    "STGeometryN",
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
                    _typeMappingSource.FindMapping(typeof(IGeometry), instance.TypeMapping.StoreType),
                    false);
            }

            return null;
        }
    }
}

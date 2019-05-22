// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    public class SqliteGeometryCollectionMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _item = typeof(IGeometryCollection).GetRuntimeProperty("Item").GetMethod;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqliteGeometryCollectionMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            if (Equals(method.OnInterface(typeof(IGeometryCollection)), _item))
            {
                return _sqlExpressionFactory.Function(
                    "GeometryN",
                    new SqlExpression[] {
                        instance,
                        _sqlExpressionFactory.Add(
                            arguments[0],
                            _sqlExpressionFactory.Constant(1))
                    },
                    method.ReturnType);
            }

            return null;
        }
    }
}

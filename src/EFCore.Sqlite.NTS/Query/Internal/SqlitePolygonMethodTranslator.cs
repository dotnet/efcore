// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    public class SqlitePolygonMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _getInteriorRingN
            = typeof(Polygon).GetRuntimeMethod(nameof(Polygon.GetInteriorRingN), new[] { typeof(int) });

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlitePolygonMethodTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));

            if (Equals(method, _getInteriorRingN))
            {
                return _sqlExpressionFactory.Function(
                    "InteriorRingN",
                    new[] { instance, _sqlExpressionFactory.Add(arguments[0], _sqlExpressionFactory.Constant(1)) },
                    nullResultAllowed: true,
                    argumentsPropagateNullability: new [] { true, true },
                    method.ReturnType);
            }

            return null;
        }
    }
}

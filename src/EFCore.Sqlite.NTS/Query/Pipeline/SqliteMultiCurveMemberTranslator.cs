// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    public class SqliteMultiCurveMemberTranslator : IMemberTranslator
    {
        private static readonly MemberInfo _isClosed = typeof(IMultiCurve).GetRuntimeProperty(nameof(IMultiCurve.IsClosed));
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqliteMultiCurveMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            if (Equals(member.OnInterface(typeof(IMultiCurve)), _isClosed))
            {
                return _sqlExpressionFactory.Case(
                    new[] {
                        new CaseWhenClause(
                            _sqlExpressionFactory.IsNotNull(instance),
                            _sqlExpressionFactory.Function(
                                "IsClosed",
                                new[] { instance },
                                returnType))
                    },
                    null);
            }

            return null;
        }
    }
}

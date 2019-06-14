// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerMultiLineStringMemberTranslator : IMemberTranslator
    {
        private static readonly MemberInfo _isClosed = typeof(MultiLineString).GetRuntimeProperty(nameof(MultiLineString.IsClosed));
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerMultiLineStringMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            if (Equals(member, _isClosed))
            {
                return _sqlExpressionFactory.Function(
                    instance,
                    "STIsClosed",
                    Array.Empty<SqlExpression>(),
                    returnType);
            }

            return null;
        }
    }
}

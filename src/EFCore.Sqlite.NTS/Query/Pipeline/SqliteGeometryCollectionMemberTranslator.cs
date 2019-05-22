// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    public class SqliteGeometryCollectionMemberTranslator : IMemberTranslator
    {
        private static readonly MemberInfo _count = typeof(IGeometryCollection).GetRuntimeProperty(nameof(IGeometryCollection.Count));
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqliteGeometryCollectionMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            return Equals(member.OnInterface(typeof(IGeometryCollection)), _count)
                ? _sqlExpressionFactory.Function("NumGeometries", new[] { instance }, returnType)
                : null;
        }
    }
}

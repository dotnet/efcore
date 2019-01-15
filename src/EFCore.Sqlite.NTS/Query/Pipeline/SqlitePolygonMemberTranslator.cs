// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    public class SqlitePolygonMemberTranslator : IMemberTranslator
    {
        private static readonly IDictionary<MemberInfo, string> _memberToFunctionName
            = new Dictionary<MemberInfo, string>
            {
                { typeof(IPolygon).GetRuntimeProperty(nameof(IPolygon.ExteriorRing)), "ExteriorRing" },
                { typeof(IPolygon).GetRuntimeProperty(nameof(IPolygon.NumInteriorRings)), "NumInteriorRing" }
            };
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlitePolygonMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            return _memberToFunctionName.TryGetValue(member.OnInterface(typeof(IPolygon)), out var functionName)
                ? _sqlExpressionFactory.Function(functionName, new[] { instance }, returnType)
                : null;
        }
    }
}

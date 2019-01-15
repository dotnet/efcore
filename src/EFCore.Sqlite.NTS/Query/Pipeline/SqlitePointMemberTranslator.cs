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
    public class SqlitePointMemberTranslator : IMemberTranslator
    {
        private static readonly IDictionary<MemberInfo, string> _memberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { typeof(IPoint).GetRuntimeProperty(nameof(IPoint.M)), "M" },
            { typeof(IPoint).GetRuntimeProperty(nameof(IPoint.X)), "X" },
            { typeof(IPoint).GetRuntimeProperty(nameof(IPoint.Y)), "Y" },
            { typeof(IPoint).GetRuntimeProperty(nameof(IPoint.Z)), "Z" }
        };
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlitePointMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            return _memberToFunctionName.TryGetValue(member.OnInterface(typeof(IPoint)), out var functionName)
                ? _sqlExpressionFactory.Function(functionName, new[] { instance }, returnType)
                : null;
        }
    }
}

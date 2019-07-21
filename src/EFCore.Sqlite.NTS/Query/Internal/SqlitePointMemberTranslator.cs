// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    public class SqlitePointMemberTranslator : IMemberTranslator
    {
        private static readonly IDictionary<MemberInfo, string> _memberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { typeof(Point).GetRuntimeProperty(nameof(Point.M)), "M" },
            { typeof(Point).GetRuntimeProperty(nameof(Point.X)), "X" },
            { typeof(Point).GetRuntimeProperty(nameof(Point.Y)), "Y" },
            { typeof(Point).GetRuntimeProperty(nameof(Point.Z)), "Z" }
        };
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlitePointMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            return _memberToFunctionName.TryGetValue(member, out var functionName)
                ? _sqlExpressionFactory.Function(functionName, new[] { instance }, returnType)
                : null;
        }
    }
}

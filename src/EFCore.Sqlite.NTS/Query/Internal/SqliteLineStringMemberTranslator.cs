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
    public class SqliteLineStringMemberTranslator : IMemberTranslator
    {
        private static readonly IDictionary<MemberInfo, string> _memberToFunctionName
            = new Dictionary<MemberInfo, string>
            {
                { typeof(LineString).GetRuntimeProperty(nameof(LineString.Count)), "NumPoints" },
                { typeof(LineString).GetRuntimeProperty(nameof(LineString.EndPoint)), "EndPoint" },
                { typeof(LineString).GetRuntimeProperty(nameof(LineString.IsClosed)), "IsClosed" },
                { typeof(LineString).GetRuntimeProperty(nameof(LineString.IsRing)), "IsRing" },
                { typeof(LineString).GetRuntimeProperty(nameof(LineString.StartPoint)), "StartPoint" }
            };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqliteLineStringMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            if (_memberToFunctionName.TryGetValue(member, out var functionName))
            {
                SqlExpression translation = _sqlExpressionFactory.Function(
                    functionName, new[] { instance }, returnType);

                if (returnType == typeof(bool))
                {
                    translation = _sqlExpressionFactory.Case(
                        new[]
                        {
                            new CaseWhenClause(
                                _sqlExpressionFactory.IsNotNull(instance),
                                translation)
                        },
                        null);
                }

                return translation;
            }

            return null;
        }
    }
}

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
    public class SqliteCurveMemberTranslator : IMemberTranslator
    {
        private static readonly IDictionary<MemberInfo, string> _memberToFunctionName
            = new Dictionary<MemberInfo, string>
            {
                { typeof(ICurve).GetRuntimeProperty(nameof(ICurve.EndPoint)), "EndPoint" },
                { typeof(ICurve).GetRuntimeProperty(nameof(ICurve.IsClosed)), "IsClosed" },
                { typeof(ICurve).GetRuntimeProperty(nameof(ICurve.IsRing)), "IsRing" },
                { typeof(ICurve).GetRuntimeProperty(nameof(ICurve.StartPoint)), "StartPoint" }
            };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqliteCurveMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            if (_memberToFunctionName.TryGetValue(member.OnInterface(typeof(ICurve)), out var functionName))
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

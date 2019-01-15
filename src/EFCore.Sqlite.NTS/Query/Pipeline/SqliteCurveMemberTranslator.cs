// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

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

        private readonly IRelationalTypeMappingSource _typeMappingSource;

        public SqliteCurveMemberTranslator(IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            if (_memberToFunctionName.TryGetValue(member.OnInterface(typeof(ICurve)), out var functionName))
            {
                SqlExpression translation = new SqlFunctionExpression(
                    functionName,
                    new[] {
                        instance
                    },
                    returnType,
                    _typeMappingSource.FindMapping(returnType),
                    false);

                if (returnType == typeof(bool))
                {
                    translation = new CaseExpression(
                        new[]
                        {
                            new CaseWhenClause(
                                new SqlNullExpression(instance, true, _typeMappingSource.FindMapping(typeof(bool))),
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

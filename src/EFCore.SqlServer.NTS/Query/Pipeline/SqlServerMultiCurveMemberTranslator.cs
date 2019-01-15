// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerMultiCurveMemberTranslator : IMemberTranslator
    {
        private static readonly MemberInfo _isClosed = typeof(IMultiCurve).GetRuntimeProperty(nameof(IMultiCurve.IsClosed));
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        public SqlServerMultiCurveMemberTranslator(IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            if (Equals(member.OnInterface(typeof(IMultiCurve)), _isClosed))
            {
                return new SqlFunctionExpression(
                    instance,
                    "STIsClosed",
                    null,
                    returnType,
                    _typeMappingSource.FindMapping(returnType),
                    false);
            }

            return null;
        }
    }
}

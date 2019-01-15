// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerGeometryCollectionMemberTranslator : IMemberTranslator
    {
        private static readonly MemberInfo _count = typeof(IGeometryCollection).GetRuntimeProperty(nameof(IGeometryCollection.Count));
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        public SqlServerGeometryCollectionMemberTranslator(IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            if (Equals(member.OnInterface(typeof(IGeometryCollection)), _count))
            {
                return new SqlFunctionExpression(
                    instance,
                    "STNumGeometries",
                    Enumerable.Empty<SqlExpression>(),
                    returnType,
                    _typeMappingSource.FindMapping(returnType),
                    false);
            }

            return null;
        }
    }
}

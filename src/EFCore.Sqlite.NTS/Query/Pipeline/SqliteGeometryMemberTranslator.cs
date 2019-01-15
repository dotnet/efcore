// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    public class SqliteGeometryMemberTranslator : IMemberTranslator
    {
        private static readonly IDictionary<MemberInfo, string> _memberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Area)), "Area" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Boundary)), "Boundary" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Centroid)), "Centroid" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Dimension)), "Dimension" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Envelope)), "Envelope" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.InteriorPoint)), "PointOnSurface" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.IsEmpty)), "IsEmpty" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.IsSimple)), "IsSimple" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.IsValid)), "IsValid" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Length)), "GLength" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.NumGeometries)), "NumGeometries" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.NumPoints)), "NumPoints" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.PointOnSurface)), "PointOnSurface" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.SRID)), "SRID" }
        };

        private static readonly MemberInfo _geometryType = typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.GeometryType));
        private static readonly MemberInfo _ogcGeometryType = typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.OgcGeometryType));
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        public SqliteGeometryMemberTranslator(IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            member = member.OnInterface(typeof(IGeometry));
            if (_memberToFunctionName.TryGetValue(member, out var functionName))
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

            if (Equals(member, _geometryType))
            {
                var stringTypeMapping = _typeMappingSource.FindMapping(returnType);

                return new CaseExpression(
                    new SqlFunctionExpression(
                        "rtrim",
                        new SqlExpression[]
                        {
                            new SqlFunctionExpression(
                                "GeometryType",
                                new[] {
                                    instance
                                },
                                returnType,
                                stringTypeMapping,
                                false),
                            MakeSqlConstant(" ZM", stringTypeMapping)
                        },
                        returnType,
                        stringTypeMapping,
                        false),
                    new CaseWhenClause(MakeSqlConstant("POINT", stringTypeMapping), MakeSqlConstant("Point", stringTypeMapping)),
                    new CaseWhenClause(MakeSqlConstant("LINESTRING", stringTypeMapping), MakeSqlConstant("LineString", stringTypeMapping)),
                    new CaseWhenClause(MakeSqlConstant("POLYGON", stringTypeMapping), MakeSqlConstant("Polygon", stringTypeMapping)),
                    new CaseWhenClause(MakeSqlConstant("MULTIPOINT", stringTypeMapping), MakeSqlConstant("MultiPoint", stringTypeMapping)),
                    new CaseWhenClause(MakeSqlConstant("MULTILINESTRING", stringTypeMapping), MakeSqlConstant("MultiLineString", stringTypeMapping)),
                    new CaseWhenClause(MakeSqlConstant("MULTIPOLYGON", stringTypeMapping), MakeSqlConstant("MultiPolygon", stringTypeMapping)),
                    new CaseWhenClause(MakeSqlConstant("GEOMETRYCOLLECTION", stringTypeMapping), MakeSqlConstant("GeometryCollection", stringTypeMapping)));
            }

            if (Equals(member, _ogcGeometryType))
            {
                var stringTypeMapping = _typeMappingSource.FindMapping(typeof(string));
                var typeMapping = _typeMappingSource.FindMapping(returnType);

                return new CaseExpression(
                    new SqlFunctionExpression(
                        "rtrim",
                        new SqlExpression[]
                        {
                            new SqlFunctionExpression(
                                "GeometryType",
                                new[] {
                                    instance
                                },
                                returnType,
                                stringTypeMapping,
                                false),
                            MakeSqlConstant(" ZM", stringTypeMapping)
                        },
                        returnType,
                        stringTypeMapping,
                        false),
                    new CaseWhenClause(MakeSqlConstant("POINT", stringTypeMapping), MakeSqlConstant(OgcGeometryType.Point, typeMapping)),
                    new CaseWhenClause(MakeSqlConstant("LINESTRING", stringTypeMapping), MakeSqlConstant(OgcGeometryType.LineString, typeMapping)),
                    new CaseWhenClause(MakeSqlConstant("POLYGON", stringTypeMapping), MakeSqlConstant(OgcGeometryType.Polygon, typeMapping)),
                    new CaseWhenClause(MakeSqlConstant("MULTIPOINT", stringTypeMapping), MakeSqlConstant(OgcGeometryType.MultiPoint, typeMapping)),
                    new CaseWhenClause(MakeSqlConstant("MULTILINESTRING", stringTypeMapping), MakeSqlConstant(OgcGeometryType.MultiLineString, typeMapping)),
                    new CaseWhenClause(MakeSqlConstant("MULTIPOLYGON", stringTypeMapping), MakeSqlConstant(OgcGeometryType.MultiPolygon, typeMapping)),
                    new CaseWhenClause(MakeSqlConstant("GEOMETRYCOLLECTION", stringTypeMapping), MakeSqlConstant(OgcGeometryType.GeometryCollection, typeMapping)));
            }

            return null;
        }

        private SqlConstantExpression MakeSqlConstant(object value, RelationalTypeMapping typeMapping)
        {
            return new SqlConstantExpression(Expression.Constant(value), typeMapping);
        }
    }
}

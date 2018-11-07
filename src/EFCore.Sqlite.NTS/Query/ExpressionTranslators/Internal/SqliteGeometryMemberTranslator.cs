// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            var member = memberExpression.Member.OnInterface(typeof(IGeometry));
            if (_memberToFunctionName.TryGetValue(member, out var functionName))
            {
                Expression newExpression = new SqlFunctionExpression(
                    functionName,
                    memberExpression.Type,
                    new[] { memberExpression.Expression });
                if (memberExpression.Type == typeof(bool))
                {
                    newExpression = new CaseExpression(
                        new CaseWhenClause(
                            Expression.Not(new IsNullExpression(memberExpression.Expression)),
                            newExpression));
                }

                return newExpression;
            }
            if (Equals(member, _geometryType))
            {
                return new CaseExpression(
                    new SqlFunctionExpression(
                        "rtrim",
                        memberExpression.Type,
                        new Expression[]
                        {
                            new SqlFunctionExpression(
                                "GeometryType",
                                memberExpression.Type,
                                new[] { memberExpression.Expression }),
                            Expression.Constant(" ZM")
                        }),
                    new CaseWhenClause(Expression.Constant("POINT"), Expression.Constant("Point")),
                    new CaseWhenClause(Expression.Constant("LINESTRING"), Expression.Constant("LineString")),
                    new CaseWhenClause(Expression.Constant("POLYGON"), Expression.Constant("Polygon")),
                    new CaseWhenClause(Expression.Constant("MULTIPOINT"), Expression.Constant("MultiPoint")),
                    new CaseWhenClause(Expression.Constant("MULTILINESTRING"), Expression.Constant("MultiLineString")),
                    new CaseWhenClause(Expression.Constant("MULTIPOLYGON"), Expression.Constant("MultiPolygon")),
                    new CaseWhenClause(Expression.Constant("GEOMETRYCOLLECTION"), Expression.Constant("GeometryCollection")));
            }
            if (Equals(member, _ogcGeometryType))
            {
                return new CaseExpression(
                    new SqlFunctionExpression(
                        "rtrim",
                        typeof(string),
                        new Expression[]
                        {
                            new SqlFunctionExpression(
                                "GeometryType",
                                typeof(string),
                                new[] { memberExpression.Expression }),
                            Expression.Constant(" ZM")
                        }),
                    new CaseWhenClause(Expression.Constant("POINT"), Expression.Constant(OgcGeometryType.Point)),
                    new CaseWhenClause(Expression.Constant("LINESTRING"), Expression.Constant(OgcGeometryType.LineString)),
                    new CaseWhenClause(Expression.Constant("POLYGON"), Expression.Constant(OgcGeometryType.Polygon)),
                    new CaseWhenClause(Expression.Constant("MULTIPOINT"), Expression.Constant(OgcGeometryType.MultiPoint)),
                    new CaseWhenClause(Expression.Constant("MULTILINESTRING"), Expression.Constant(OgcGeometryType.MultiLineString)),
                    new CaseWhenClause(Expression.Constant("MULTIPOLYGON"), Expression.Constant(OgcGeometryType.MultiPolygon)),
                    new CaseWhenClause(Expression.Constant("GEOMETRYCOLLECTION"), Expression.Constant(OgcGeometryType.GeometryCollection)));
            }

            return null;
        }
    }
}

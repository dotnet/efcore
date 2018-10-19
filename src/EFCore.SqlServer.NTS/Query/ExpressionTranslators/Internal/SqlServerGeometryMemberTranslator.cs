// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerGeometryMemberTranslator : IMemberTranslator
    {
        private static readonly IDictionary<MemberInfo, string> _memberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Area)), "STArea" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Dimension)), "STDimension" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.GeometryType)), "STGeometryType" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.IsEmpty)), "STIsEmpty" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.IsValid)), "STIsValid" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Length)), "STLength" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.NumGeometries)), "STNumGeometries" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.NumPoints)), "STNumPoints" }
        };

        private static readonly IDictionary<MemberInfo, string> _geometryMemberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Boundary)), "STBoundary" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Centroid)), "STCentroid" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Envelope)), "STEnvelope" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.InteriorPoint)), "STPointOnSurface" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.IsSimple)), "STIsSimple" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.PointOnSurface)), "STPointOnSurface" }
        };

        private static readonly MemberInfo _ogcGeometryType = typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.OgcGeometryType));
        private static readonly MemberInfo _srid = typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.SRID));

        private readonly IRelationalTypeMappingSource _typeMappingSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerGeometryMemberTranslator(IRelationalTypeMappingSource typeMappingSource)
            => _typeMappingSource = typeMappingSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            if (!typeof(IGeometry).IsAssignableFrom(memberExpression.Member.DeclaringType))
            {
                return null;
            }

            var storeType = memberExpression.FindSpatialStoreType();
            var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

            var member = memberExpression.Member.OnInterface(typeof(IGeometry));
            if (_memberToFunctionName.TryGetValue(member, out var functionName)
                || (!isGeography && _geometryMemberToFunctionName.TryGetValue(member, out functionName)))
            {
                RelationalTypeMapping resultTypeMapping = null;
                if (typeof(IGeometry).IsAssignableFrom(memberExpression.Type))
                {
                    resultTypeMapping = _typeMappingSource.FindMapping(memberExpression.Type, storeType);
                }

                return new SqlFunctionExpression(
                    memberExpression.Expression,
                    functionName,
                    memberExpression.Type,
                    Enumerable.Empty<Expression>(),
                    resultTypeMapping);
            }
            if (Equals(member, _ogcGeometryType))
            {
                var whenThenList = new List<CaseWhenClause>()
                {
                    new CaseWhenClause(Expression.Constant("Point"), Expression.Constant(OgcGeometryType.Point)),
                    new CaseWhenClause(Expression.Constant("LineString"), Expression.Constant(OgcGeometryType.LineString)),
                    new CaseWhenClause(Expression.Constant("Polygon"), Expression.Constant(OgcGeometryType.Polygon)),
                    new CaseWhenClause(Expression.Constant("MultiPoint"), Expression.Constant(OgcGeometryType.MultiPoint)),
                    new CaseWhenClause(Expression.Constant("MultiLineString"), Expression.Constant(OgcGeometryType.MultiLineString)),
                    new CaseWhenClause(Expression.Constant("MultiPolygon"), Expression.Constant(OgcGeometryType.MultiPolygon)),
                    new CaseWhenClause(Expression.Constant("GeometryCollection"), Expression.Constant(OgcGeometryType.GeometryCollection)),
                    new CaseWhenClause(Expression.Constant("CircularString"), Expression.Constant(OgcGeometryType.CircularString)),
                    new CaseWhenClause(Expression.Constant("CompoundCurve"), Expression.Constant(OgcGeometryType.CompoundCurve)),
                    new CaseWhenClause(Expression.Constant("CurvePolygon"), Expression.Constant(OgcGeometryType.CurvePolygon))
                };
                if (isGeography)
                {
                    whenThenList.Add(new CaseWhenClause(Expression.Constant("FullGlobe"), Expression.Constant((OgcGeometryType)126)));
                }

                return new CaseExpression(
                    new SqlFunctionExpression(
                        memberExpression.Expression,
                        "STGeometryType",
                        typeof(string),
                        Enumerable.Empty<Expression>()),
                    whenThenList.ToArray());
            }
            if (Equals(member, _srid))
            {
                return new SqlFunctionExpression(
                    memberExpression.Expression,
                    "STSrid",
                    memberExpression.Type,
                    niladic: true);
            }

            return null;
        }
    }
}

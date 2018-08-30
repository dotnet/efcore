// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

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
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Boundary)), "STBoundary" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Centroid)), "STCentroid" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Dimension)), "STDimension" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Envelope)), "STEnvelope" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.GeometryType)), "STGeometryType" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.IsEmpty)), "STIsEmpty" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.IsSimple)), "STIsSimple" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.IsValid)), "STIsValid" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Length)), "STLength" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.NumGeometries)), "STNumGeometries" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.NumPoints)), "STNumPoints" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.PointOnSurface)), "STPointOnSurface" }
        };

        private static readonly MemberInfo _srid = typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.SRID));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            var member = memberExpression.Member.OnInterface(typeof(IGeometry));
            if (_memberToFunctionName.TryGetValue(member, out var functionName))
            {
                return new SqlFunctionExpression(
                    memberExpression.Expression,
                    functionName,
                    memberExpression.Type,
                    Enumerable.Empty<Expression>());
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

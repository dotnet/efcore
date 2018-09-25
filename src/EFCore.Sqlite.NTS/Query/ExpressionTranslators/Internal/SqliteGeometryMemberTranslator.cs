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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Expression Translate(MemberExpression memberExpression)
        {
            var member = memberExpression.Member.OnInterface(typeof(IGeometry));
            if (_memberToFunctionName.TryGetValue(member, out var functionName))
            {
                return new SqlFunctionExpression(
                    functionName,
                    memberExpression.Type,
                    new[] { memberExpression.Expression });
            }

            return null;
        }
    }
}

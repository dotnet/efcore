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
    public class SqlServerPolygonMemberTranslator : IMemberTranslator
    {
        private static readonly MemberInfo _exteriorRing = typeof(IPolygon).GetRuntimeProperty(nameof(IPolygon.ExteriorRing));
        private static readonly MemberInfo _numInteriorRings = typeof(IPolygon).GetRuntimeProperty(nameof(IPolygon.NumInteriorRings));

        private static readonly IDictionary<MemberInfo, string> _geometryMemberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { _exteriorRing, "STExteriorRing" },
            { _numInteriorRings, "STNumInteriorRing" }
        };

        private readonly IRelationalTypeMappingSource _typeMappingSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerPolygonMemberTranslator(IRelationalTypeMappingSource typeMappingSource)
            => _typeMappingSource = typeMappingSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            if (!typeof(IPolygon).IsAssignableFrom(memberExpression.Member.DeclaringType))
            {
                return null;
            }

            var storeType = memberExpression.FindSpatialStoreType();
            var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

            var member = memberExpression.Member.OnInterface(typeof(IPolygon));
            if (isGeography)
            {
                if (Equals(_exteriorRing, member))
                {
                    return new SqlFunctionExpression(
                        memberExpression.Expression,
                        "RingN",
                        memberExpression.Type,
                        new[] { Expression.Constant(1) },
                        _typeMappingSource.FindMapping(typeof(ILineString), storeType));
                }
                else if (Equals(_numInteriorRings, member))
                {
                    return Expression.Subtract(
                        new SqlFunctionExpression(
                            memberExpression.Expression,
                            "NumRings",
                            memberExpression.Type,
                            Enumerable.Empty<Expression>()),
                        Expression.Constant(1));
                }
            }
            else if (_geometryMemberToFunctionName.TryGetValue(member, out var functionName))
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

            return null;
        }
    }
}

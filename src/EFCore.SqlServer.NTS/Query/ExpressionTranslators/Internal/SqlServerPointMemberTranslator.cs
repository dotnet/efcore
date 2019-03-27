// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerPointMemberTranslator : IMemberTranslator
    {
        private static readonly IDictionary<MemberInfo, string> _memberToPropertyName = new Dictionary<MemberInfo, string>
        {
            { typeof(IPoint).GetRuntimeProperty(nameof(IPoint.M)), "M" },
            { typeof(IPoint).GetRuntimeProperty(nameof(IPoint.Z)), "Z" }
        };

        private static readonly IDictionary<MemberInfo, string> _geographyMemberToPropertyName = new Dictionary<MemberInfo, string>
        {
            { typeof(IPoint).GetRuntimeProperty(nameof(IPoint.X)), "Long" },
            { typeof(IPoint).GetRuntimeProperty(nameof(IPoint.Y)), "Lat" }
        };

        private static readonly IDictionary<MemberInfo, string> _geometryMemberToPropertyName = new Dictionary<MemberInfo, string>
        {
            { typeof(IPoint).GetRuntimeProperty(nameof(IPoint.X)), "STX" },
            { typeof(IPoint).GetRuntimeProperty(nameof(IPoint.Y)), "STY" }
        };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            if (!typeof(IPoint).IsAssignableFrom(memberExpression.Member.DeclaringType))
            {
                return null;
            }

            var storeType = memberExpression.FindSpatialStoreType();
            var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

            var member = memberExpression.Member.OnInterface(typeof(IPoint));
            if (_memberToPropertyName.TryGetValue(member, out var propertyName)
                || (isGeography
                    ? _geographyMemberToPropertyName.TryGetValue(member, out propertyName)
                    : _geometryMemberToPropertyName.TryGetValue(member, out propertyName)))
            {
                return new SqlFunctionExpression(
                    memberExpression.Expression,
                    propertyName,
                    memberExpression.Type,
                    niladic: true);
            }

            return null;
        }
    }
}

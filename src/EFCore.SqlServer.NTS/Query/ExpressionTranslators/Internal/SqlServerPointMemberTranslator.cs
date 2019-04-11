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
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     <para>
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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

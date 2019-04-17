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
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class SqlServerCurveMemberTranslator : IMemberTranslator
    {
        private static readonly IDictionary<MemberInfo, string> _memberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { typeof(ICurve).GetRuntimeProperty(nameof(ICurve.EndPoint)), "STEndPoint" },
            { typeof(ICurve).GetRuntimeProperty(nameof(ICurve.IsClosed)), "STIsClosed" },
            { typeof(ICurve).GetRuntimeProperty(nameof(ICurve.StartPoint)), "STStartPoint" }
        };

        private static readonly MemberInfo _isRing = typeof(ICurve).GetRuntimeProperty(nameof(ICurve.IsRing));

        private readonly IRelationalTypeMappingSource _typeMappingSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerCurveMemberTranslator(IRelationalTypeMappingSource typeMappingSource)
            => _typeMappingSource = typeMappingSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            if (!typeof(ICurve).IsAssignableFrom(memberExpression.Member.DeclaringType))
            {
                return null;
            }

            var storeType = memberExpression.FindSpatialStoreType();
            var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

            var member = memberExpression.Member.OnInterface(typeof(ICurve));
            if (_memberToFunctionName.TryGetValue(member, out var functionName))
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

            if (!isGeography
                && Equals(member, _isRing))
            {
                return new SqlFunctionExpression(
                    memberExpression.Expression,
                    "STIsRing",
                    memberExpression.Type,
                    Enumerable.Empty<Expression>());
            }

            return null;
        }
    }
}

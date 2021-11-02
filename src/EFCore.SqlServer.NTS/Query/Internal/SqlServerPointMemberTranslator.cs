// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    internal class SqlServerPointMemberTranslator : IMemberTranslator
    {
        private static readonly IDictionary<MemberInfo, string> _memberToPropertyName = new Dictionary<MemberInfo, string>
        {
            { typeof(Point).GetRequiredRuntimeProperty(nameof(Point.M)), "M" },
            { typeof(Point).GetRequiredRuntimeProperty(nameof(Point.Z)), "Z" }
        };

        private static readonly IDictionary<MemberInfo, string> _geographyMemberToPropertyName = new Dictionary<MemberInfo, string>
        {
            { typeof(Point).GetRequiredRuntimeProperty(nameof(Point.X)), "Long" },
            { typeof(Point).GetRequiredRuntimeProperty(nameof(Point.Y)), "Lat" }
        };

        private static readonly IDictionary<MemberInfo, string> _geometryMemberToPropertyName = new Dictionary<MemberInfo, string>
        {
            { typeof(Point).GetRequiredRuntimeProperty(nameof(Point.X)), "STX" },
            { typeof(Point).GetRequiredRuntimeProperty(nameof(Point.Y)), "STY" }
        };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerPointMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression? Translate(
            SqlExpression? instance,
            MemberInfo member,
            Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (typeof(Point).IsAssignableFrom(member.DeclaringType))
            {
                Check.DebugAssert(instance!.TypeMapping != null, "Instance must have typeMapping assigned.");
                var storeType = instance.TypeMapping.StoreType;
                var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

                if (_memberToPropertyName.TryGetValue(member, out var propertyName)
                    || (isGeography
                        ? _geographyMemberToPropertyName.TryGetValue(member, out propertyName)
                        : _geometryMemberToPropertyName.TryGetValue(member, out propertyName))
                    && propertyName != null)
                {
                    return _sqlExpressionFactory.NiladicFunction(
                        instance,
                        propertyName,
                        nullable: true,
                        instancePropagatesNullability: true,
                        returnType);
                }
            }

            return null;
        }
    }
}

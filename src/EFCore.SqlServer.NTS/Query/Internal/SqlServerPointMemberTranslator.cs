// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
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
            { typeof(Point).GetRuntimeProperty(nameof(Point.M)), "M" }, { typeof(Point).GetRuntimeProperty(nameof(Point.Z)), "Z" }
        };

        private static readonly IDictionary<MemberInfo, string> _geographyMemberToPropertyName = new Dictionary<MemberInfo, string>
        {
            { typeof(Point).GetRuntimeProperty(nameof(Point.X)), "Long" }, { typeof(Point).GetRuntimeProperty(nameof(Point.Y)), "Lat" }
        };

        private static readonly IDictionary<MemberInfo, string> _geometryMemberToPropertyName = new Dictionary<MemberInfo, string>
        {
            { typeof(Point).GetRuntimeProperty(nameof(Point.X)), "STX" }, { typeof(Point).GetRuntimeProperty(nameof(Point.Y)), "STY" }
        };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerPointMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            if (typeof(Point).IsAssignableFrom(member.DeclaringType))
            {
                Check.DebugAssert(instance.TypeMapping != null, "Instance must have typeMapping assigned.");
                var storeType = instance.TypeMapping.StoreType;
                var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

                if (_memberToPropertyName.TryGetValue(member, out var propertyName)
                    || (isGeography
                        ? _geographyMemberToPropertyName.TryGetValue(member, out propertyName)
                        : _geometryMemberToPropertyName.TryGetValue(member, out propertyName)))
                {
                    return _sqlExpressionFactory.Function(
                        instance,
                        propertyName,
                        nullResultAllowed: true,
                        instancePropagatesNullability: true,
                        returnType);
                }
            }

            return null;
        }
    }
}

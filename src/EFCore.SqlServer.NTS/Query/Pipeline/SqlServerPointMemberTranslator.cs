// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    internal class SqlServerPointMemberTranslator : IMemberTranslator
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

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerPointMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            if (typeof(IPoint).IsAssignableFrom(member.DeclaringType))
            {
                Debug.Assert(instance.TypeMapping != null, "Instance must have typeMapping assigned.");
                var storeType = instance.TypeMapping.StoreType;
                var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

                member = member.OnInterface(typeof(IPoint));
                if (_memberToPropertyName.TryGetValue(member, out var propertyName)
                    || (isGeography
                        ? _geographyMemberToPropertyName.TryGetValue(member, out propertyName)
                        : _geometryMemberToPropertyName.TryGetValue(member, out propertyName)))
                {
                    return _sqlExpressionFactory.Function(
                        instance,
                        propertyName,
                        true,
                        returnType);
                }
            }

            return null;
        }
    }
}

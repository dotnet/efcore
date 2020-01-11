// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerLineStringMemberTranslator : IMemberTranslator
    {
        private static readonly IDictionary<MemberInfo, string> _memberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { typeof(LineString).GetRuntimeProperty(nameof(LineString.Count)), "STNumPoints" },
            { typeof(LineString).GetRuntimeProperty(nameof(LineString.EndPoint)), "STEndPoint" },
            { typeof(LineString).GetRuntimeProperty(nameof(LineString.IsClosed)), "STIsClosed" },
            { typeof(LineString).GetRuntimeProperty(nameof(LineString.StartPoint)), "STStartPoint" },
            { typeof(LineString).GetRuntimeProperty(nameof(LineString.IsRing)), "STIsRing" }
        };

        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerLineStringMemberTranslator(
            [NotNull] IRelationalTypeMappingSource typeMappingSource,
            [NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _typeMappingSource = typeMappingSource;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            Check.NotNull(member, nameof(member));
            Check.NotNull(returnType, nameof(returnType));

            if (_memberToFunctionName.TryGetValue(member, out var functionName))
            {
                Check.DebugAssert(instance.TypeMapping != null, "Instance must have typeMapping assigned.");
                var storeType = instance.TypeMapping.StoreType;
                var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

                if (isGeography && string.Equals(functionName, "STIsRing"))
                {
                    return null;
                }

                var resultTypeMapping = typeof(Geometry).IsAssignableFrom(returnType)
                    ? _typeMappingSource.FindMapping(returnType, storeType)
                    : _typeMappingSource.FindMapping(returnType);

                return _sqlExpressionFactory.Function(
                    instance,
                    functionName,
                    Enumerable.Empty<SqlExpression>(),
                    nullResultAllowed: true,
                    instancePropagatesNullability: true,
                    argumentsPropagateNullability: Enumerable.Empty<bool>(),
                    returnType,
                    resultTypeMapping);
            }

            return null;
        }
    }
}

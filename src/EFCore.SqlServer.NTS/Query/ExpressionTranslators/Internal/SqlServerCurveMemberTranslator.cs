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

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerCurveMemberTranslator : IMemberTranslator
    {
        private static readonly IDictionary<MemberInfo, string> _memberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { typeof(ICurve).GetRuntimeProperty(nameof(ICurve.EndPoint)), "STEndPoint" },
            { typeof(ICurve).GetRuntimeProperty(nameof(ICurve.IsClosed)), "STIsClosed" },
            { typeof(ICurve).GetRuntimeProperty(nameof(ICurve.StartPoint)), "STStartPoint" },
        };

        private static readonly MemberInfo _isRing = typeof(ICurve).GetRuntimeProperty(nameof(ICurve.IsRing));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            var instance = memberExpression.Expression;
            var isGeography = string.Equals(
                instance.FindProperty(instance.Type)?.Relational().ColumnType,
                "geography",
                StringComparison.OrdinalIgnoreCase);

            var member = memberExpression.Member.OnInterface(typeof(ICurve));
            if (_memberToFunctionName.TryGetValue(member, out var functionName))
            {
                return new SqlFunctionExpression(
                    instance,
                    functionName,
                    memberExpression.Type,
                    Enumerable.Empty<Expression>());
            }
            else if (!isGeography && Equals(member, _isRing))
            {
                return new SqlFunctionExpression(
                    instance,
                    "STIsRing",
                    memberExpression.Type,
                    Enumerable.Empty<Expression>());
            }

            return null;
        }
    }
}

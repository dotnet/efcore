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
    public class SqlitePointMemberTranslator : IMemberTranslator
    {
        private static readonly IDictionary<MemberInfo, string> _memberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { typeof(IPoint).GetRuntimeProperty(nameof(IPoint.M)), "M" },
            { typeof(IPoint).GetRuntimeProperty(nameof(IPoint.X)), "X" },
            { typeof(IPoint).GetRuntimeProperty(nameof(IPoint.Y)), "Y" },
            { typeof(IPoint).GetRuntimeProperty(nameof(IPoint.Z)), "Z" }
        };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            var member = memberExpression.Member.OnInterface(typeof(IPoint));
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

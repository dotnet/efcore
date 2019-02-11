// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.ExpressionTranslators.Internal
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
    public class SqliteCurveMemberTranslator : IMemberTranslator
    {
        private static readonly IDictionary<MemberInfo, string> _memberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { typeof(ICurve).GetRuntimeProperty(nameof(ICurve.EndPoint)), "EndPoint" },
            { typeof(ICurve).GetRuntimeProperty(nameof(ICurve.IsClosed)), "IsClosed" },
            { typeof(ICurve).GetRuntimeProperty(nameof(ICurve.IsRing)), "IsRing" },
            { typeof(ICurve).GetRuntimeProperty(nameof(ICurve.StartPoint)), "StartPoint" }
        };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            var member = memberExpression.Member.OnInterface(typeof(ICurve));
            if (_memberToFunctionName.TryGetValue(member, out var functionName))
            {
                Expression newExpression = new SqlFunctionExpression(
                    functionName,
                    memberExpression.Type,
                    new[] { memberExpression.Expression });
                if (memberExpression.Type == typeof(bool))
                {
                    newExpression = new CaseExpression(
                        new CaseWhenClause(
                            Expression.Not(new IsNullExpression(memberExpression.Expression)),
                            newExpression));
                }

                return newExpression;
            }

            return null;
        }
    }
}

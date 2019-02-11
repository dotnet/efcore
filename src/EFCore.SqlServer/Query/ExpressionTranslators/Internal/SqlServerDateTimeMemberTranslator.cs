// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
    public class SqlServerDateTimeMemberTranslator : IMemberTranslator
    {
        private static readonly Dictionary<string, string> _datePartMapping
            = new Dictionary<string, string>
            {
                { nameof(DateTime.Year), "year" },
                { nameof(DateTime.Month), "month" },
                { nameof(DateTime.DayOfYear), "dayofyear" },
                { nameof(DateTime.Day), "day" },
                { nameof(DateTime.Hour), "hour" },
                { nameof(DateTime.Minute), "minute" },
                { nameof(DateTime.Second), "second" },
                { nameof(DateTime.Millisecond), "millisecond" }
            };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            var declaringType = memberExpression.Member.DeclaringType;
            if (declaringType == typeof(DateTime)
                || declaringType == typeof(DateTimeOffset))
            {
                var memberName = memberExpression.Member.Name;

                if (_datePartMapping.TryGetValue(memberName, out var datePart))
                {
                    return new SqlFunctionExpression(
                        "DATEPART",
                        memberExpression.Type,
                        arguments: new[] { new SqlFragmentExpression(datePart), memberExpression.Expression });
                }

                switch (memberName)
                {
                    case nameof(DateTime.Now):
                        return declaringType == typeof(DateTimeOffset)
                            ? new SqlFunctionExpression("SYSDATETIMEOFFSET", memberExpression.Type)
                            : new SqlFunctionExpression("GETDATE", memberExpression.Type);

                    case nameof(DateTime.UtcNow):
                        return declaringType == typeof(DateTimeOffset)
                            ? (Expression)new ExplicitCastExpression(
                                new SqlFunctionExpression("SYSUTCDATETIME", memberExpression.Type),
                                typeof(DateTimeOffset))
                            : new SqlFunctionExpression("GETUTCDATE", memberExpression.Type);

                    case nameof(DateTime.Date):
                        return new SqlFunctionExpression(
                            "CONVERT",
                            memberExpression.Type,
                            new[] { new SqlFragmentExpression("date"), memberExpression.Expression });

                    case nameof(DateTime.Today):
                        return new SqlFunctionExpression(
                            "CONVERT",
                            memberExpression.Type,
                            new Expression[] { new SqlFragmentExpression("date"), new SqlFunctionExpression("GETDATE", memberExpression.Type) });

                    case nameof(DateTime.TimeOfDay):
                        return new ExplicitCastExpression(
                            memberExpression.Expression,
                            memberExpression.Type);
                }
            }

            return null;
        }
    }
}

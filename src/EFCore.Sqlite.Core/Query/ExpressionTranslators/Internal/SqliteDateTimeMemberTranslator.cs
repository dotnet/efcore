// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteDateTimeMemberTranslator : IMemberTranslator
    {
        private static readonly Dictionary<string, string> _datePartMapping
            = new Dictionary<string, string>
            {
                { nameof(DateTime.Year), "'%Y'" },
                { nameof(DateTime.Month), "'%m'" },
                { nameof(DateTime.DayOfYear), "'%j'" },
                { nameof(DateTime.Day), "'%d'" },
                { nameof(DateTime.Hour), "'%H'" },
                { nameof(DateTime.Minute), "'%M'" },
                { nameof(DateTime.Second), "'%S'" },
            };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            if (memberExpression.Member.DeclaringType == typeof(DateTime))
            {
                var memberName = memberExpression.Member.Name;

                if (_datePartMapping.TryGetValue(memberName, out var datePart))
                {
                    return new ExplicitCastExpression(
                        new SqlFunctionExpression(
                            "strftime",
                            memberExpression.Type,
                            new[] {
                                new SqlFragmentExpression(datePart),
                                memberExpression.Expression
                            }),
                        typeof(int));
                }

                var sqlArguments = new List<Expression>
                {
                    new SqlFragmentExpression("'%Y-%m-%d %H:%M:%f'")
                };

                switch (memberName)
                {
                    case nameof(DateTime.Now):
                        sqlArguments.Add(new SqlFragmentExpression("'now'"));
                        sqlArguments.Add(new SqlFragmentExpression("'localtime'"));
                        break;

                    case nameof(DateTime.UtcNow):
                        sqlArguments.Add(new SqlFragmentExpression("'now'"));
                        break;

                    case nameof(DateTime.Date):
                        sqlArguments.Add(memberExpression.Expression);
                        sqlArguments.Add(new SqlFragmentExpression("'start of day'"));
                        break;

                    case nameof(DateTime.Today):
                        sqlArguments.Add(new SqlFragmentExpression("'now'"));
                        sqlArguments.Add(new SqlFragmentExpression("'localtime'"));
                        sqlArguments.Add(new SqlFragmentExpression("'start of day'"));
                        break;
                }

                if (sqlArguments.Count > 1)
                {
                    return new SqlFunctionExpression(
                        "rtrim",
                        memberExpression.Type,
                        new Expression[] {
                            new SqlFunctionExpression(
                                "rtrim",
                                memberExpression.Type,
                                new Expression[]
                                {
                                    new SqlFunctionExpression(
                                        "strftime",
                                        memberExpression.Type,
                                        sqlArguments),
                                    new SqlFragmentExpression("'0'")
                                }),
                            new SqlFragmentExpression("'.'")
                        });
                }
            }

            return null;
        }
    }
}

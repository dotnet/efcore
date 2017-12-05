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
    public class SqliteDateTimeDatePartComponentTranslator : IMemberTranslator
    {
        private readonly Dictionary<string, string> _datePartDictionary = new Dictionary<string, string>
        {
            { nameof(DateTime.Day) , "%d" },
            { nameof(DateTime.Month) , "%m" },
            { nameof(DateTime.Year) , "%Y" },
            { nameof(DateTime.Hour) , "%H" },
            { nameof(DateTime.Minute) , "%M" },
            { nameof(DateTime.Second) , "%S" },
            { nameof(DateTime.Millisecond) , "year" },
            { nameof(DateTime.DayOfWeek) , "%w" },
            { nameof(DateTime.DayOfYear) , "%j" }
        };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            if (memberExpression.Expression != null
                && (memberExpression.Expression.Type == typeof(DateTime))
                && _datePartDictionary.TryGetValue(memberExpression.Member.Name,out var datePart))
            {
                return
                    new ExplicitCastExpression(
                        new SqlFunctionExpression(
                            functionName: "strftime",
                            returnType: memberExpression.Type,
                            arguments: new[]
                            {
                                new SqlFragmentExpression($"'{datePart}'"),
                                memberExpression.Expression
                            }),
                        memberExpression.Type);
            }
            return null;
        }
    }
}

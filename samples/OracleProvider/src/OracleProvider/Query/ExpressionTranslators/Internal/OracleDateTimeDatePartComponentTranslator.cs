// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class OracleDateTimeDatePartComponentTranslator : IMemberTranslator
    {
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            string datePart;
            if (memberExpression.Expression != null
                && (memberExpression.Expression.Type == typeof(DateTime) || memberExpression.Expression.Type == typeof(DateTimeOffset))
                && (datePart = GetDatePart(memberExpression.Member.Name)) != null)
            {
                return new SqlFunctionExpression(
                    functionName: "EXTRACT",
                    returnType: memberExpression.Type,
                    arguments: new[]
                    {
                        new SqlFragmentExpression(datePart),
                        memberExpression.Expression
                    });
            }
            return null;
        }

        private static string GetDatePart(string memberName)
        {
            switch (memberName)
            {
                case nameof(DateTime.Year):
                    return "YEAR";
                case nameof(DateTime.Month):
                    return "MONTH";
                case nameof(DateTime.Day):
                    return "DAY";
                case nameof(DateTime.Hour):
                    return "HOUR";
                case nameof(DateTime.Minute):
                    return "MINUTE";
                case nameof(DateTime.Second):
                    return "SECOND";
                default:
                    return null;
            }
        }
    }
}

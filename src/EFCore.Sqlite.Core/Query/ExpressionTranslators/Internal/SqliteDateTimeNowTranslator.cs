// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteDateTimeNowTranslator : IMemberTranslator
    {
        private static string _sqliteFormatDate = "'%Y-%m-%d %H:%M:%S'";
        private static string _sqliteFunctionDateFormat = "strftime";
        private static string _sqliteLocalTime = "'localtime'";
        private static string _sqliteNow = "'now'";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            if (memberExpression.Expression == null
                && memberExpression.Member.DeclaringType == typeof(DateTime))
            {
                var sqlArguments = new List<Expression>
                {
                    new SqlFragmentExpression(_sqliteFormatDate),
                    new SqlFragmentExpression(_sqliteNow),
                    new SqlFragmentExpression(_sqliteLocalTime)
                };

                switch (memberExpression.Member.Name)
                {
                    case nameof(DateTime.Now): 
                        return new SqlFunctionExpression(
                            _sqliteFunctionDateFormat,
                            memberExpression.Type,
                            sqlArguments);
                    case nameof(DateTime.UtcNow):
                        return new SqlFunctionExpression(
                            _sqliteFunctionDateFormat,
                            memberExpression.Type,
                            sqlArguments.Take(2));
                }
            }

            return null;
        }
    }
}

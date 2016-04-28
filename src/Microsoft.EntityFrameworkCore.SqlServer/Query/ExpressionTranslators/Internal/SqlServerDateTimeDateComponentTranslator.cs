// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class SqlServerDateTimeDateComponentTranslator : IMemberTranslator
    {
        public virtual Expression Translate(MemberExpression memberExpression)
            => (memberExpression.Expression != null)
            && (memberExpression.Expression.Type == typeof(DateTime))
            && (memberExpression.Member.Name == nameof(DateTime.Date))
            ? new SqlFunctionExpression("CONVERT", 
                memberExpression.Type, 
                new[]
                {
                   Expression.Constant(DbType.Date),
                   memberExpression.Expression
                })
            : null;
    }
}
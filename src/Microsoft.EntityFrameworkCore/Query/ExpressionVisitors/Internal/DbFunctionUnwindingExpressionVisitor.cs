// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DbFunctionUnwindingExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitExtension(Expression node)
        {
            var dbFunctionExpression = node as DbFunctionExpression;
            if (dbFunctionExpression != null)
            {
                var mce = dbFunctionExpression.OriginalExpression as MethodCallExpression;

                if (mce != null)
                {
                    var newArguments = Visit(new ReadOnlyCollection<Expression>(mce.Arguments));

                    return Expression.Call(mce.Object, mce.Method, newArguments);
                }
            }

            return node;
        }
    }
}

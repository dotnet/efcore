// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.ExpressionTranslators;

namespace Microsoft.Data.Entity.SqlServer.Query.ExpressionTranslators
{
    public class DateTimeNowTranslator : IMemberTranslator
    {
        public virtual Expression Translate([NotNull] MemberExpression memberExpression)
        {
            if (memberExpression.Expression == null
                && memberExpression.Member.DeclaringType == typeof(DateTime)
                && memberExpression.Member.Name == nameof(DateTime.Now))
            {
                return new SqlFunctionExpression("GETDATE", Enumerable.Empty<Expression>(), memberExpression.Type);
            }
            else if (memberExpression.Member.Name == nameof(DateTime.UtcNow))
            {
                return new SqlFunctionExpression("GETUTCDATE", Enumerable.Empty<Expression>(), memberExpression.Type);
            }

            return null;
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Query.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionTranslators.Internal
{
    public class SqlServerDateTimeNowTranslator : IMemberTranslator
    {
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            if (memberExpression.Expression == null
                && memberExpression.Member.DeclaringType == typeof(DateTime))
            {
                switch (memberExpression.Member.Name)
                {
                    case nameof(DateTime.Now):
                        return new SqlFunctionExpression("GETDATE", memberExpression.Type);
                    case nameof(DateTime.UtcNow):
                        return new SqlFunctionExpression("GETUTCDATE", memberExpression.Type);
                }
            }

            return null;
        }
    }
}

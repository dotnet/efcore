// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors
{
    class AnonymousObjectAccessSimplifyingVisitor : RelinqExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            if (memberExpression.Expression is NewExpression newExpression
                && newExpression.Type.Name.Contains("__AnonymousType"))
            {
                var matchingMemberIndex = newExpression.Members.Select((m, i) => new { match = m == memberExpression.Member, i }).Where(r => r.match).Single().i;

                return newExpression.Arguments[matchingMemberIndex];
            }

            return base.VisitMember(memberExpression);
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            if (constantExpression.Type == typeof(AnonymousObject))
            {
                var anonymousObjectValue = (AnonymousObject)constantExpression.Value;

                if (anonymousObjectValue.OnlyNullValues(out var count))
                {
                    return NavigationExpansionHelpers.CreateNullKeyExpression(typeof(AnonymousObject), count);
                }
            }

            return base.VisitConstant(constantExpression);
        }
    }
}

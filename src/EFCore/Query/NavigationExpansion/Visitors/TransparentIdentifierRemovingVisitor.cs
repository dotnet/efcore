// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors
{
    // TODO: temporary hack to simplify relinq parsing, can be removed once relinq is gone
    public class TransparentIdentifierRemovingVisitor : RelinqExpressionVisitor
    {
        private static Expression ExtractFromTransparentIdentifier(MemberExpression memberExpression, Stack<string> extractionPath)
        {
            if (memberExpression.Member.Name == "Outer"
                || memberExpression.Member.Name == "Inner")
            {
                extractionPath.Push(memberExpression.Member.Name);

                if (memberExpression.Expression is MemberExpression innerMember)
                {
                    return ExtractFromTransparentIdentifier(innerMember, extractionPath);
                }
                else
                {
                    var result = memberExpression.Expression;
                    while (extractionPath.Count > 0)
                    {
                        if (!(result is NewExpression))
                        {
                            if (extractionPath.Count == 0)
                            {
                                return memberExpression;
                            }

                            var expr = Expression.Field(result, extractionPath.Pop());
                            while (extractionPath.Count > 0)
                            {
                                expr = Expression.Field(expr, extractionPath.Pop());
                            }

                            return expr;
                        }

                        var extractionPathElement = extractionPath.Pop();

                        var newExpression = (NewExpression)result;

                        if (extractionPathElement == "Outer")
                        {
                            result = newExpression.Arguments[0];
                        }
                        else
                        {
                            result = newExpression.Arguments[1];
                        }
                    }

                    return result;
                }
            }

            return memberExpression;
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            if (memberExpression.Member.Name == "Outer"
                || memberExpression.Member.Name == "Inner")
            {
                var result = ExtractFromTransparentIdentifier(memberExpression, new Stack<string>());

                return result;
            }
            else
            {
                return base.VisitMember(memberExpression);
            }
        }

        protected override Expression VisitSubQuery(SubQueryExpression subQueryExpression)
        {
            subQueryExpression.QueryModel.TransformExpressions(Visit);

            return base.VisitSubQuery(subQueryExpression);
        }
    }
}

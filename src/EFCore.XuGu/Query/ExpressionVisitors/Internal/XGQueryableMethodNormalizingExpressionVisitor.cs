// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal;

// TODO: 9.0
// Remove XGQueryableMethodNormalizingExpressionVisitor, XGBipolarExpression and
// XGQueryTranslationPreprocessor.NormalizeQueryableMethod (or the whole class) and use ElementAt() directly in Json translation classes.

/// <summary>
/// Skips normalization of array[index].Property to array.Select(e => e.Property).ElementAt(index),
/// because it messes-up our JSON-Array handling in `XGSqlTranslatingExpressionVisitor`.
/// See https://github.com/dotnet/efcore/issues/30386.
/// </summary>
public class XGQueryableMethodNormalizingExpressionVisitor : QueryableMethodNormalizingExpressionVisitor
{
    public XGQueryableMethodNormalizingExpressionVisitor(QueryCompilationContext queryCompilationContext)
        : base(queryCompilationContext, isEfConstantSupported: true)
    {
    }

    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        // Convert array[x] to array.ElementAt(x)
        if (binaryExpression is
            {
                NodeType: ExpressionType.ArrayIndex,
                Left: var source,
                Right: var index
            })
        {
            return new XGBipolarExpression(
                base.VisitBinary(binaryExpression),
                binaryExpression.Update(
                    Visit(binaryExpression.Left),
                    VisitAndConvert(binaryExpression.Conversion, nameof(VisitBinary)),
                    Visit(binaryExpression.Right)));

            // Original (base) implementation:
            //
            // return VisitMethodCall(
            //     Expression.Call(
            //         ElementAtMethodInfo.MakeGenericMethod(source.Type.GetSequenceType()), source, index));
        }

        return base.VisitBinary(binaryExpression);
    }

    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        // Normalize list[x] to list.ElementAt(x)
        if (methodCallExpression is
            {
                Method:
                {
                    Name: "get_Item",
                    IsStatic: false,
                    DeclaringType: Type declaringType
                },
                Object: Expression indexerSource,
                Arguments: [var index]
            }
            && declaringType.GetInterface("IReadOnlyList`1") is not null)
        {
            return new XGBipolarExpression(
                base.VisitMethodCall(methodCallExpression),
                methodCallExpression.Update(
                    Visit(methodCallExpression.Object),
                    VisitArguments(methodCallExpression.Arguments)));

            IEnumerable<Expression> VisitArguments(IEnumerable<Expression> arguments)
            {
                foreach (var expression in arguments)
                {
                    yield return Visit(expression);
                }
            }

            // Original (base) implementation:
            //
            // return VisitMethodCall(
            //     Expression.Call(
            //         ElementAtMethodInfo.MakeGenericMethod(indexerSource.Type.GetSequenceType()),
            //         indexerSource,
            //         index));
        }

        return base.VisitMethodCall(methodCallExpression);
    }
}

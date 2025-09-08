// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Expressions.Internal;

public class XGBipolarExpression : Expression
{
    public XGBipolarExpression(
        Expression defaultExpression,
        Expression alternativeExpression)
    {
        Check.NotNull(defaultExpression, nameof(defaultExpression));
        Check.NotNull(alternativeExpression, nameof(alternativeExpression));

        DefaultExpression = defaultExpression;
        AlternativeExpression = alternativeExpression;
    }

    public virtual Expression DefaultExpression { get; }
    public virtual Expression AlternativeExpression { get; }

    public override ExpressionType NodeType
        => ExpressionType.UnaryPlus;

    public override Type Type
        => DefaultExpression.Type;

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var defaultExpression = visitor.Visit(DefaultExpression);
        var alternativeExpression = visitor.Visit(AlternativeExpression);

        return Update(defaultExpression, alternativeExpression);
    }

    public virtual XGBipolarExpression Update(Expression defaultExpression, Expression alternativeExpression)
        => defaultExpression != DefaultExpression || alternativeExpression != AlternativeExpression
            ? new XGBipolarExpression(defaultExpression, alternativeExpression)
            : this;

    public override string ToString()
    {
        var expressionPrinter = new ExpressionPrinter();

        expressionPrinter.AppendLine("<XGBipolarExpression>(");

        using (expressionPrinter.Indent())
        {
            expressionPrinter.Append("Default: ");
            expressionPrinter.Visit(DefaultExpression);
            expressionPrinter.AppendLine();

            expressionPrinter.Append("Alternative: ");
            expressionPrinter.Visit(AlternativeExpression);
            expressionPrinter.AppendLine();
        }

        expressionPrinter.Append(")");

        return expressionPrinter.ToString();
    }

    public override bool Equals(object obj)
        => obj != null
           && (ReferenceEquals(this, obj)
               || obj is XGBipolarExpression bipolarExpression
               && Equals(bipolarExpression));

    private bool Equals(XGBipolarExpression bipolarExpression)
        => base.Equals(bipolarExpression)
           && DefaultExpression.Equals(bipolarExpression.DefaultExpression)
           && AlternativeExpression.Equals(bipolarExpression.AlternativeExpression);

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), DefaultExpression, AlternativeExpression);
}

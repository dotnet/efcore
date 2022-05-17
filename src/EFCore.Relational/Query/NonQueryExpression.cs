// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class NonQueryExpression : Expression, IPrintableExpression
{
    public NonQueryExpression(DeleteExpression deleteExpression)
    {
        DeleteExpression = deleteExpression;
    }

    public virtual DeleteExpression DeleteExpression { get; }

    /// <inheritdoc />
    public override Type Type => typeof(int);

    /// <inheritdoc />
    public sealed override ExpressionType NodeType => ExpressionType.Extension;

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var deleteExpression = (DeleteExpression)visitor.Visit(DeleteExpression);

        return Update(deleteExpression);
    }

    public virtual NonQueryExpression Update(DeleteExpression deleteExpression)
        => deleteExpression != DeleteExpression
            ? new NonQueryExpression(deleteExpression)
            : this;

    /// <inheritdoc />
    public virtual void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append($"({nameof(NonQueryExpression)}: ");
        expressionPrinter.Visit(DeleteExpression);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is NonQueryExpression nonQueryExpression
                && Equals(nonQueryExpression));

    private bool Equals(NonQueryExpression nonQueryExpression)
        => DeleteExpression == nonQueryExpression.DeleteExpression;

    /// <inheritdoc />
    public override int GetHashCode() => DeleteExpression.GetHashCode();
}

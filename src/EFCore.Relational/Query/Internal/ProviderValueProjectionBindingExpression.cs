// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

// Reads a projected value as the database returned it, without the type mapping's value converter. Identifiers are compared on the
// client to tell rows apart as the database did; through a converter, the same database value can materialize as instances that the
// model type's comparer doesn't consider equal (a class without Equals compares by reference), or as different values altogether (a
// write converter that isn't deterministic), so a converted identifier is read and compared as its provider value instead.
// Never survives shaper processing.
internal sealed class ProviderValueProjectionBindingExpression : Expression, IPrintableExpression
{
    public ProviderValueProjectionBindingExpression(ProjectionBindingExpression projectionBinding, RelationalTypeMapping typeMapping)
    {
        ProjectionBinding = projectionBinding;
        TypeMapping = typeMapping;
        Type = typeMapping.Converter!.ProviderClrType.MakeNullable();
    }

    // The binding to the projected column, read with its converter everywhere else.
    public ProjectionBindingExpression ProjectionBinding { get; }

    // The column's type mapping, whose converter the read skips.
    public RelationalTypeMapping TypeMapping { get; }

    public override Type Type { get; }

    public override ExpressionType NodeType
        => ExpressionType.Extension;

    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => visitor.Visit(ProjectionBinding) is var projectionBinding && projectionBinding != ProjectionBinding
            ? new ProviderValueProjectionBindingExpression((ProjectionBindingExpression)projectionBinding, TypeMapping)
            : this;

    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("PROVIDER_VALUE(");
        expressionPrinter.Visit(ProjectionBinding);
        expressionPrinter.Append(")");
    }
}

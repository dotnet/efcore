// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

public sealed class ConcreteColumnExpression : ColumnExpression
{
    private readonly TableReferenceExpression _table;

    public ConcreteColumnExpression(IProperty property, IColumnBase column, TableReferenceExpression table, bool nullable)
        : this(
            column.Name,
            table,
            property.ClrType.UnwrapNullableType(),
            column.PropertyMappings.First(m => m.Property == property).TypeMapping,
            nullable || column.IsNullable)
    {
    }

    public ConcreteColumnExpression(ProjectionExpression subqueryProjection, TableReferenceExpression table)
        : this(
            subqueryProjection.Alias, table,
            subqueryProjection.Type, subqueryProjection.Expression.TypeMapping!,
            IsNullableProjection(subqueryProjection))
    {
    }

    private static bool IsNullableProjection(ProjectionExpression projectionExpression)
        => projectionExpression.Expression switch
        {
            ColumnExpression columnExpression => columnExpression.IsNullable,
            SqlConstantExpression sqlConstantExpression => sqlConstantExpression.Value == null,
            _ => true
        };

    public ConcreteColumnExpression(
        string name,
        TableReferenceExpression table,
        Type type,
        RelationalTypeMapping typeMapping,
        bool nullable)
        : base(type, typeMapping)
    {
        Name = name;
        _table = table;
        IsNullable = nullable;
    }

    public override string Name { get; }

    public override TableExpressionBase Table
        => _table.Table;

    public override string TableAlias
        => _table.Alias;

    public override bool IsNullable { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public SelectExpression SelectExpression => _table.SelectExpression;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    public override ConcreteColumnExpression MakeNullable()
        => IsNullable ? this : new ConcreteColumnExpression(Name, _table, Type, TypeMapping!, true);

    public void UpdateTableReference(SelectExpression oldSelect, SelectExpression newSelect)
        => _table.UpdateTableReference(oldSelect, newSelect);

    internal void Verify(IReadOnlyList<TableReferenceExpression> tableReferences)
    {
        if (!tableReferences.Contains(_table, ReferenceEqualityComparer.Instance))
        {
            throw new InvalidOperationException("Dangling column.");
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
           && (ReferenceEquals(this, obj)
               || obj is ConcreteColumnExpression concreteColumnExpression
               && Equals(concreteColumnExpression));

    private bool Equals(ConcreteColumnExpression concreteColumnExpression)
        => base.Equals(concreteColumnExpression)
           && Name == concreteColumnExpression.Name
           && _table.Equals(concreteColumnExpression._table)
           && IsNullable == concreteColumnExpression.IsNullable;

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Name, _table, IsNullable);
}

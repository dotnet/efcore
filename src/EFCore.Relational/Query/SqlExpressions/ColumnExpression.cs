// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a column in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
[DebuggerDisplay("{TableAlias}.{Name}")]
public class ColumnExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="ColumnExpression" /> class.
    /// </summary>
    /// <param name="name">The name of the column.</param>
    /// <param name="tableAlias">The alias of the table to which this column refers.</param>
    /// <param name="type">The <see cref="System.Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    /// <param name="nullable">Whether this expression represents a nullable column.</param>
    public ColumnExpression(
        string name,
        string tableAlias,
        Type type,
        RelationalTypeMapping? typeMapping,
        bool nullable)
        : base(type, typeMapping)
    {
        Name = name;
        TableAlias = tableAlias;
        IsNullable = nullable;
    }

    /// <summary>
    ///     The name of the column.
    /// </summary>
    public virtual string Name { get; }

    /// <summary>
    ///     The alias of the table from which column is being referenced.
    /// </summary>
    public virtual string TableAlias { get; }

    /// <summary>
    ///     The bool value indicating if this column can have null values.
    /// </summary>
    public virtual bool IsNullable { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <summary>
    ///     Makes this column nullable.
    /// </summary>
    /// <returns>A new expression which has <see cref="IsNullable" /> property set to true.</returns>
    public virtual ColumnExpression MakeNullable()
        => IsNullable ? this : new ColumnExpression(Name, TableAlias, Type, TypeMapping, true);

    /// <summary>
    ///     Applies supplied type mapping to this expression.
    /// </summary>
    /// <param name="typeMapping">A relational type mapping to apply.</param>
    /// <returns>A new expression which has supplied type mapping.</returns>
    public virtual SqlExpression ApplyTypeMapping(RelationalTypeMapping? typeMapping)
        => new ColumnExpression(Name, TableAlias, Type, typeMapping, IsNullable);

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(ColumnExpression).GetConstructor(
                [typeof(string), typeof(string), typeof(Type), typeof(RelationalTypeMapping), typeof(bool)])!,
            Constant(Name),
            Constant(TableAlias),
            Constant(Type),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping),
            Constant(IsNullable));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Append(TableAlias).Append(".").Append(Name);

    /// <inheritdoc />
    public override string ToString()
        => $"{TableAlias}.{Name}";

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is ColumnExpression columnExpression
                && Equals(columnExpression));

    private bool Equals(ColumnExpression columnExpression)
        => base.Equals(columnExpression)
            && Name == columnExpression.Name
            && TableAlias == columnExpression.TableAlias
            && IsNullable == columnExpression.IsNullable;

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Name, TableAlias, IsNullable);
}

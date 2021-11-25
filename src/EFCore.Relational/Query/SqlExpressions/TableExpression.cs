// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     An expression that represents a table or view in a SQL tree.
/// </summary>
/// <remarks>
///     This is a simple wrapper around a table and schema name. Instances of this type cannot be constructed by
///     application or database provider code. If this is a problem for your application or provider, then please file
///     an issue at <see href="https://github.com/dotnet/efcore">github.com/dotnet/efcore</see>.
/// </remarks>
public sealed class TableExpression : TableExpressionBase, IClonableTableExpressionBase
{
    internal TableExpression(ITableBase table)
        : this(table, annotations: null)
    {
    }

    private TableExpression(ITableBase table, IEnumerable<IAnnotation>? annotations)
        : base(alias: table.Name[..1].ToLowerInvariant(), annotations)
    {
        Name = table.Name;
        Schema = table.Schema;
        Table = table;
    }

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        if (!string.IsNullOrEmpty(Schema))
        {
            expressionPrinter.Append(Schema).Append(".");
        }

        expressionPrinter.Append(Name);
        PrintAnnotations(expressionPrinter);

        expressionPrinter.Append(" AS ").Append(Alias);
    }

    /// <summary>
    ///     The alias assigned to this table source.
    /// </summary>
    [NotNull]
    public override string? Alias
    {
        get => base.Alias!;
        internal set => base.Alias = value;
    }

    /// <summary>
    ///     The name of the table or view.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The schema of the table or view.
    /// </summary>
    public string? Schema { get; }

    /// <summary>
    ///     The <see cref="ITableBase" /> associated with this table or view.
    /// </summary>
    public ITableBase Table { get; }

    /// <inheritdoc />
    public TableExpressionBase Clone()
        => new TableExpression(Table, GetAnnotations()) { Alias = Alias };

    /// <inheritdoc />
    public override bool Equals(object? obj)
        // This should be reference equal only.
        => obj != null && ReferenceEquals(this, obj);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Name, Schema);
}

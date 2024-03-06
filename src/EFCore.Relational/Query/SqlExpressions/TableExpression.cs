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
public sealed class TableExpression : TableExpressionBase, ITableBasedExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="TableExpression" /> class.
    /// </summary>
    public TableExpression(string alias, ITableBase table)
        : this(alias, table, annotations: null)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public TableExpression(string alias, ITableBase table, IReadOnlyDictionary<string, IAnnotation>? annotations)
        : base(alias, annotations)
    {
        Name = table.Name;
        Schema = table.Schema;
        Table = table;
    }

    /// <summary>
    ///     The alias assigned to this table source.
    /// </summary>
    public override string Alias
        => base.Alias!;

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
    ITableBase ITableBasedExpression.Table
        => Table;

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(TableExpression)
                .GetConstructor([typeof(string), typeof(ITableBase), typeof(IReadOnlyDictionary<string, IAnnotation>)])!,
            Constant(Alias, typeof(string)),
            RelationalExpressionQuotingUtilities.QuoteTableBase(Table),
            RelationalExpressionQuotingUtilities.QuoteAnnotations(Annotations));

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

    /// <inheritdoc />
    public override TableExpressionBase Clone(string? alias, ExpressionVisitor cloningExpressionVisitor)
        => new TableExpression(alias!, Table, Annotations);

    /// <inheritdoc />
    protected override TableExpression WithAnnotations(IReadOnlyDictionary<string, IAnnotation> annotations)
        => new(Alias, Table, annotations);

    /// <inheritdoc />
    public override TableExpression WithAlias(string newAlias)
        => new(newAlias, Table, Annotations);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is TableExpression fromSqlExpression
                && Equals(fromSqlExpression));

    private bool Equals(TableExpression fromSqlExpression)
        => base.Equals(fromSqlExpression)
            && Table == fromSqlExpression.Table;

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Name, Schema);
}

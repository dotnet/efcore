// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class TpcTablesExpression : TableExpressionBase
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TpcTablesExpression(
        string? alias,
        IEntityType entityType,
        IReadOnlyList<SelectExpression> subSelectExpressions,
        ColumnExpression discriminatorColumn,
        List<string> discriminatorValues)
        : base(alias)
    {
        EntityType = entityType;
        SelectExpressions = subSelectExpressions;
        DiscriminatorColumn = discriminatorColumn;
        DiscriminatorValues = discriminatorValues;
    }

    private TpcTablesExpression(
        string? alias,
        IEntityType entityType,
        IReadOnlyList<SelectExpression> subSelectExpressions,
        ColumnExpression discriminatorColumn,
        List<string> discriminatorValues,
        IReadOnlyDictionary<string, IAnnotation>? annotations)
        : base(alias, annotations)
    {
        EntityType = entityType;
        SelectExpressions = subSelectExpressions;
        DiscriminatorColumn = discriminatorColumn;
        DiscriminatorValues = discriminatorValues;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string Alias
        => base.Alias!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IEntityType EntityType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IReadOnlyList<SelectExpression> SelectExpressions { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ColumnExpression DiscriminatorColumn { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // Note: this gets mutated from SelectExpression.ApplyPredicate, during which the SelectExpression is still in mutable state;
    // so that's "OK".
    public List<string> DiscriminatorValues { get; internal set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TpcTablesExpression Prune(IReadOnlyList<string> discriminatorValues)
    {
        var subSelectExpressions = discriminatorValues.Count == 0
            ? [SelectExpressions[0]]
            : SelectExpressions.Where(
                se =>
                    discriminatorValues.Contains((string)((SqlConstantExpression)se.Projection[^1].Expression).Value!)).ToList();

        Check.DebugAssert(subSelectExpressions.Count > 0, "TPC must have at least 1 table selected.");

        return new TpcTablesExpression(Alias, EntityType, subSelectExpressions, DiscriminatorColumn, DiscriminatorValues, Annotations);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        // This is implementation detail hence visitors are not supposed to see inside the sub-selects unless they really need to.
        var visitedColumn = (ColumnExpression)visitor.Visit(DiscriminatorColumn);
        return visitedColumn == DiscriminatorColumn
            ? this
            : new(Alias, EntityType, SelectExpressions, visitedColumn, DiscriminatorValues, Annotations);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override TpcTablesExpression WithAnnotations(IReadOnlyDictionary<string, IAnnotation> annotations)
        => new(Alias, EntityType, SelectExpressions, DiscriminatorColumn, DiscriminatorValues, annotations);

    /// <inheritdoc />
    public override TpcTablesExpression WithAlias(string newAlias)
        => new(newAlias, EntityType, SelectExpressions, DiscriminatorColumn, DiscriminatorValues, Annotations);

    /// <inheritdoc />
    public override Expression Quote()
        => throw new UnreachableException("TpcTablesExpression is a temporary tree representation and should never be quoted");

    /// <inheritdoc />
    public override TableExpressionBase Clone(string? alias, ExpressionVisitor cloningExpressionVisitor)
    {
        var subSelectExpressions = SelectExpressions.Select(cloningExpressionVisitor.Visit).ToList<SelectExpression>();
        var discriminatorColumn = (ColumnExpression)cloningExpressionVisitor.Visit(DiscriminatorColumn);
        return new TpcTablesExpression(alias, EntityType, subSelectExpressions, discriminatorColumn, DiscriminatorValues, Annotations);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine("(");
        using (expressionPrinter.Indent())
        {
            expressionPrinter.VisitCollection(SelectExpressions, e => e.AppendLine().AppendLine("UNION ALL"));
        }

        expressionPrinter.AppendLine()
            .AppendLine(") AS " + Alias);
        PrintAnnotations(expressionPrinter);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is TpcTablesExpression tpcTablesExpression
                && Equals(tpcTablesExpression));

    private bool Equals(TpcTablesExpression tpcTablesExpression)
    {
        if (!base.Equals(tpcTablesExpression)
            || EntityType != tpcTablesExpression.EntityType)
        {
            return false;
        }

        return SelectExpressions.SequenceEqual(tpcTablesExpression.SelectExpressions);
    }

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), EntityType);
}

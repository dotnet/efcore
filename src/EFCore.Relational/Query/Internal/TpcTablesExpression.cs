// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class TpcTablesExpression : TableExpressionBase
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TpcTablesExpression(
        string? alias, IEntityType entityType, IReadOnlyList<SelectExpression> subSelectExpressions)
        : base(alias)
    {
        EntityType = entityType;
        SelectExpressions = subSelectExpressions;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private TpcTablesExpression(
        string? alias,
        IEntityType entityType,
        IReadOnlyList<SelectExpression> subSelectExpressions,
        IEnumerable<IAnnotation>? annotations)
        : base(alias, annotations)
    {
        EntityType = entityType;
        SelectExpressions = subSelectExpressions;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [NotNull]
    public override string? Alias
    {
        get => base.Alias!;
        internal set => base.Alias = value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEntityType EntityType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<SelectExpression> SelectExpressions { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TpcTablesExpression Prune(IReadOnlyList<string> discriminatorValues, IReadOnlyCollection<string>? referencedColumns)
    {
        var subSelectExpressions = discriminatorValues.Count == 0
            ? new List<SelectExpression> { SelectExpressions[0] }
            : SelectExpressions.Where(se =>
                discriminatorValues.Contains((string)((SqlConstantExpression)se.Projection[^1].Expression).Value!)).ToList();

        Check.DebugAssert(subSelectExpressions.Count > 0, "TPC must have at least 1 table selected.");

        if (referencedColumns != null)
        {
            foreach (var se in subSelectExpressions)
            {
                se.Prune(referencedColumns);
            }
        }

        return new TpcTablesExpression(Alias, EntityType, subSelectExpressions, GetAnnotations());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // This is implementation detail hence visitors are not supposed to see inside unless they really need to.
    protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

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
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), EntityType);
}

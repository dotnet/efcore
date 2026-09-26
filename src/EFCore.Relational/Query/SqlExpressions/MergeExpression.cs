// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a MERGE (upsert) operation in a SQL tree: a set of source rows inserted into a target table,
///         updating rows that conflict on a declared set of columns.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally not used in application code.
///     </para>
/// </summary>
[DebuggerDisplay("{Microsoft.EntityFrameworkCore.Query.ExpressionPrinter.Print(this), nq}")]
public sealed class MergeExpression : Expression, IPrintableExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="MergeExpression" /> class.
    /// </summary>
    /// <param name="table">The target table being merged into.</param>
    /// <param name="insertColumns">The names of the columns supplied for each inserted row, aligned with <paramref name="sourceRows" />.</param>
    /// <param name="sourceRows">The source rows to merge.</param>
    /// <param name="conflictColumns">The names of the columns that determine whether a source row matches an existing target row.</param>
    /// <param name="updateSetters">The setters applied to matched rows, or <see langword="null" /> to leave matched rows unchanged.</param>
    /// <param name="returning">A projection returned for each affected row, or <see langword="null" /> when no results are returned.</param>
    /// <param name="returningShaper">The shaper that materializes each returned row, or <see langword="null" /> when no results are returned.</param>
    public MergeExpression(
        TableExpression table,
        IReadOnlyList<string> insertColumns,
        IReadOnlyList<RowValueExpression> sourceRows,
        IReadOnlyList<string> conflictColumns,
        IReadOnlyList<MergeColumnSetter>? updateSetters,
        IReadOnlyList<ProjectionExpression>? returning,
        LambdaExpression? returningShaper = null)
        : this(table, insertColumns, sourceRows, conflictColumns, updateSetters, returning, returningShaper, new HashSet<string>())
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public MergeExpression(
        TableExpression table,
        IReadOnlyList<string> insertColumns,
        IReadOnlyList<RowValueExpression> sourceRows,
        IReadOnlyList<string> conflictColumns,
        IReadOnlyList<MergeColumnSetter>? updateSetters,
        IReadOnlyList<ProjectionExpression>? returning,
        LambdaExpression? returningShaper,
        ISet<string> tags)
    {
        Table = table;
        InsertColumns = insertColumns;
        SourceRows = sourceRows;
        ConflictColumns = conflictColumns;
        UpdateSetters = updateSetters;
        Returning = returning;
        ReturningShaper = returningShaper;
        Tags = tags;
    }

    /// <summary>
    ///     The list of tags applied to this <see cref="MergeExpression" />.
    /// </summary>
    public ISet<string> Tags { get; }

    /// <summary>
    ///     The target table being merged into.
    /// </summary>
    public TableExpression Table { get; }

    /// <summary>
    ///     The names of the columns supplied for each inserted row, aligned with the values in <see cref="SourceRows" />.
    /// </summary>
    public IReadOnlyList<string> InsertColumns { get; }

    /// <summary>
    ///     The source rows to merge.
    /// </summary>
    public IReadOnlyList<RowValueExpression> SourceRows { get; }

    /// <summary>
    ///     The names of the columns that determine whether a source row matches an existing target row (the conflict target).
    /// </summary>
    public IReadOnlyList<string> ConflictColumns { get; }

    /// <summary>
    ///     The setters applied to matched rows, or <see langword="null" /> to leave matched rows unchanged (DO NOTHING).
    /// </summary>
    public IReadOnlyList<MergeColumnSetter>? UpdateSetters { get; }

    /// <summary>
    ///     A projection returned for each affected row, or <see langword="null" /> when no results are returned.
    /// </summary>
    public IReadOnlyList<ProjectionExpression>? Returning { get; }

    /// <summary>
    ///     The shaper that materializes each returned row into the result type, or <see langword="null" /> when no results are returned.
    ///     This is a CLR materialization concern carried on the expression for the shaped-query compiler; it is not part of the SQL tree
    ///     and is not visited.
    /// </summary>
    public LambdaExpression? ReturningShaper { get; }

    /// <summary>
    ///     Applies a given set of tags.
    /// </summary>
    /// <param name="tags">A list of tags to apply.</param>
    public MergeExpression ApplyTags(ISet<string> tags)
        => new(Table, InsertColumns, SourceRows, ConflictColumns, UpdateSetters, Returning, ReturningShaper, tags);

    /// <inheritdoc />
    public override Type Type
        => Returning is null ? typeof(void) : typeof(object);

    /// <inheritdoc />
    public override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var changed = false;

        var table = (TableExpression)visitor.Visit(Table);
        changed |= !ReferenceEquals(table, Table);

        var sourceRows = new RowValueExpression[SourceRows.Count];
        for (var i = 0; i < SourceRows.Count; i++)
        {
            sourceRows[i] = (RowValueExpression)visitor.Visit(SourceRows[i]);
            changed |= !ReferenceEquals(sourceRows[i], SourceRows[i]);
        }

        MergeColumnSetter[]? updateSetters = null;
        if (UpdateSetters is not null)
        {
            updateSetters = new MergeColumnSetter[UpdateSetters.Count];
            for (var i = 0; i < UpdateSetters.Count; i++)
            {
                var newValue = (SqlExpression)visitor.Visit(UpdateSetters[i].Value);
                changed |= !ReferenceEquals(newValue, UpdateSetters[i].Value);
                updateSetters[i] = ReferenceEquals(newValue, UpdateSetters[i].Value)
                    ? UpdateSetters[i]
                    : UpdateSetters[i] with { Value = newValue };
            }
        }

        ProjectionExpression[]? returning = null;
        if (Returning is not null)
        {
            returning = new ProjectionExpression[Returning.Count];
            for (var i = 0; i < Returning.Count; i++)
            {
                returning[i] = (ProjectionExpression)visitor.Visit(Returning[i]);
                changed |= !ReferenceEquals(returning[i], Returning[i]);
            }
        }

        return changed
            ? new MergeExpression(
                table,
                InsertColumns,
                sourceRows,
                ConflictColumns,
                updateSetters ?? UpdateSetters,
                returning ?? Returning,
                ReturningShaper,
                Tags)
            : this;
    }

    /// <inheritdoc />
    public void Print(ExpressionPrinter expressionPrinter)
    {
        foreach (var tag in Tags)
        {
            expressionPrinter.AppendLine($"-- {tag}");
        }

        expressionPrinter.AppendLine($"MERGE INTO {Table.Name} AS {Table.Alias}");
        expressionPrinter.AppendLine($"INSERT ({string.Join(", ", InsertColumns)})");
        expressionPrinter.AppendLine($"ON CONFLICT ({string.Join(", ", ConflictColumns)})");
        expressionPrinter.AppendLine(UpdateSetters is null ? "DO NOTHING" : "DO UPDATE");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is MergeExpression merge
            && (ReferenceEquals(this, merge)
                || (Table.Equals(merge.Table)
                    && InsertColumns.SequenceEqual(merge.InsertColumns)
                    && SourceRows.SequenceEqual(merge.SourceRows)
                    && ConflictColumns.SequenceEqual(merge.ConflictColumns)
                    && (UpdateSetters is null
                        ? merge.UpdateSetters is null
                        : merge.UpdateSetters is not null && UpdateSetters.SequenceEqual(merge.UpdateSetters))
                    && (Returning is null
                        ? merge.Returning is null
                        : merge.Returning is not null && Returning.SequenceEqual(merge.Returning))));

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Table);
        foreach (var column in InsertColumns)
        {
            hash.Add(column);
        }

        foreach (var row in SourceRows)
        {
            hash.Add(row);
        }

        foreach (var column in ConflictColumns)
        {
            hash.Add(column);
        }

        return hash.ToHashCode();
    }
}

/// <summary>
///     Represents a single column assignment applied to matched rows in a <see cref="MergeExpression" />.
/// </summary>
/// <param name="ColumnName">The target column being assigned.</param>
/// <param name="Value">The value assigned to the column.</param>
public sealed record MergeColumnSetter(string ColumnName, SqlExpression Value);

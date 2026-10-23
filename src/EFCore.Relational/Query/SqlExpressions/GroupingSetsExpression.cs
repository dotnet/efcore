// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a <c>ROLLUP</c>, <c>CUBE</c> or <c>GROUPING SETS</c> element of a SQL <c>GROUP BY</c> clause.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     This expression only appears in <see cref="SelectExpression.GroupBy" />; it isn't a scalar value and can't be projected.
///     Its type mapping is a placeholder.
/// </remarks>
public class GroupingSetsExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="GroupingSetsExpression" /> class.
    /// </summary>
    /// <param name="kind">The kind of grouping.</param>
    /// <param name="items">The grouping terms.</param>
    /// <param name="sets">
    ///     For <see cref="GroupingSetsKind.GroupingSets" />, the grouping sets, each one a list of indexes into <paramref name="items" />;
    ///     otherwise <see langword="null" />.
    /// </param>
    public GroupingSetsExpression(
        GroupingSetsKind kind,
        IReadOnlyList<SqlExpression> items,
        IReadOnlyList<IReadOnlyList<int>>? sets = null)
        : base(typeof(object), GroupingSetsTypeMapping.Default)
    {
        Check.NotEmpty(items);

        if (kind == GroupingSetsKind.GroupingSets)
        {
            Check.NotNull(sets);

            if (sets.SelectMany(s => s).Any(i => i < 0 || i >= items.Count))
            {
                throw new ArgumentOutOfRangeException(nameof(sets));
            }
        }
        else if (sets is not null)
        {
            throw new ArgumentException(null, nameof(sets));
        }

        Kind = kind;
        Items = items;
        Sets = sets;
    }

    /// <summary>
    ///     The kind of grouping.
    /// </summary>
    public virtual GroupingSetsKind Kind { get; }

    /// <summary>
    ///     The grouping terms.
    /// </summary>
    public virtual IReadOnlyList<SqlExpression> Items { get; }

    /// <summary>
    ///     For <see cref="GroupingSetsKind.GroupingSets" />, the grouping sets, each one a list of indexes into <see cref="Items" />;
    ///     otherwise <see langword="null" />.
    /// </summary>
    public virtual IReadOnlyList<IReadOnlyList<int>>? Sets { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update(visitor.VisitAndConvert(Items));

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="items">The <see cref="Items" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual GroupingSetsExpression Update(IReadOnlyList<SqlExpression> items)
        => items.Count == Items.Count && items.Zip(Items, (x, y) => (x, y)).All(tup => tup.x == tup.y)
            ? this
            : new GroupingSetsExpression(Kind, items, Sets);

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(GroupingSetsExpression).GetConstructor(
                [typeof(GroupingSetsKind), typeof(IReadOnlyList<SqlExpression>), typeof(IReadOnlyList<IReadOnlyList<int>>)])!,
            Constant(Kind),
            NewArrayInit(typeof(SqlExpression), Items.Select(i => i.Quote())),
            Sets is null
                ? Constant(null, typeof(IReadOnlyList<IReadOnlyList<int>>))
                : NewArrayInit(
                    typeof(IReadOnlyList<int>),
                    Sets.Select(s => NewArrayInit(typeof(int), s.Select(i => Constant(i))))));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        switch (Kind)
        {
            case GroupingSetsKind.Rollup:
                expressionPrinter.Append("ROLLUP (");
                expressionPrinter.VisitCollection(Items);
                expressionPrinter.Append(")");
                break;

            case GroupingSetsKind.Cube:
                expressionPrinter.Append("CUBE (");
                expressionPrinter.VisitCollection(Items);
                expressionPrinter.Append(")");
                break;

            case GroupingSetsKind.GroupingSets:
                expressionPrinter.Append("GROUPING SETS (");
                for (var i = 0; i < Sets!.Count; i++)
                {
                    if (i > 0)
                    {
                        expressionPrinter.Append(", ");
                    }

                    expressionPrinter.Append("(");
                    expressionPrinter.VisitCollection(Sets[i].Select(index => Items[index]).ToList());
                    expressionPrinter.Append(")");
                }

                expressionPrinter.Append(")");
                break;

            default:
                throw new UnreachableException();
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is GroupingSetsExpression other && Equals(other);

    private bool Equals(GroupingSetsExpression? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null
            || !base.Equals(other)
            || other.Kind != Kind
            || !other.Items.SequenceEqual(Items))
        {
            return false;
        }

        if (Sets is null || other.Sets is null)
        {
            return Sets is null && other.Sets is null;
        }

        return Sets.Count == other.Sets.Count
            && Sets.Zip(other.Sets).All(e => e.First.SequenceEqual(e.Second));
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(base.GetHashCode());
        hashCode.Add(Kind);

        foreach (var item in Items)
        {
            hashCode.Add(item);
        }

        if (Sets is not null)
        {
            foreach (var set in Sets)
            {
                hashCode.Add(set.Count);
                foreach (var index in set)
                {
                    hashCode.Add(index);
                }
            }
        }

        return hashCode.ToHashCode();
    }

    // Not a value with a database type; this only satisfies the requirement that every SQL expression has a type mapping.
    private sealed class GroupingSetsTypeMapping : RelationalTypeMapping
    {
        public static GroupingSetsTypeMapping Default { get; } = new(typeof(object));

        private GroupingSetsTypeMapping(Type clrType)
            : base("", clrType)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => this;
    }
}

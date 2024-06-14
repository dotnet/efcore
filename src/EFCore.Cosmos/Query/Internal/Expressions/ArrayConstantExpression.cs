// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     Represents an inline array in a Cosmos SQL query, e.g. [1, 2, c.Id].
/// </summary>
/// <seealso href="https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/query/constants">CosmosDB constant expressions</seealso>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
[DebuggerDisplay("{Microsoft.EntityFrameworkCore.Query.ExpressionPrinter.Print(this), nq}")]
public class ArrayConstantExpression(Type elementClrType, IReadOnlyList<SqlExpression> items, CoreTypeMapping? typeMapping = null)
    : SqlExpression(typeof(IEnumerable<>).MakeGenericType(elementClrType), typeMapping)
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<SqlExpression> Items { get; } = items;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ArrayConstantExpression VisitChildren(ExpressionVisitor visitor)
        => visitor.VisitAndConvert(Items) is var newItems
            && ReferenceEquals(newItems, Items)
                ? this
                : new ArrayConstantExpression(Type, newItems);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("[");

        var count = Items.Count;
        for (var i = 0; i < count; i++)
        {
            expressionPrinter.Visit(Items[i]);

            if (i < count - 1)
            {
                expressionPrinter.Append(", ");
            }
        }

        expressionPrinter.Append("]");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is ArrayConstantExpression other && Equals(other);

    private bool Equals(ArrayConstantExpression? other)
        => ReferenceEquals(this, other) || (base.Equals(other) && Items.SequenceEqual(other.Items));

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        foreach (var item in Items)
        {
            hashCode.Add(item);
        }

        return hashCode.ToHashCode();
    }
}

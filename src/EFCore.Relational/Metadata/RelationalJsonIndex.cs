// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Describes a relational table index defined over one or more properties contained within a
///     JSON-mapped column. Holds the JSON elements targeted by the index together with the
///     complex-collection indices traversed to reach each indexed property.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="Elements" /> list contains one <see cref="IRelationalJsonElement" /> per
///         indexed property, identifying the leaf JSON element within the JSON-mapped column.
///     </para>
///     <para>
///         The <see cref="CollectionIndices" /> list runs parallel to <see cref="Elements" /> and,
///         for each indexed property, contains an entry whose values resolve the indexers of the
///         complex-collection segments on the path to the property: a <see langword="null" />
///         entry indicates the indexer is unspecified (all elements), and a fixed value indicates a
///         specific element. A <see langword="null" /> top-level entry indicates the property is
///         not reached through any complex collection.
///     </para>
///     <para>
///         The <see cref="IsDescending" /> list, when present, runs parallel to <see cref="Elements" />
///         and indicates the sort order requested for each indexed JSON element. Sort order for a JSON
///         index can only be represented per element (since all elements may share the same JSON
///         container column, which is exposed as a single, deduplicated operation column), so it is
///         carried here rather than on the containing <see cref="CreateIndexOperation" />/<see cref="ITableIndex" />,
///         whose column-level <see cref="ITableIndex.IsDescending" /> is <see langword="null" /> for JSON indexes.
///     </para>
/// </remarks>
public sealed class RelationalJsonIndex : IEquatable<RelationalJsonIndex>
{
    /// <summary>
    ///     Creates a new <see cref="RelationalJsonIndex" /> instance.
    /// </summary>
    /// <param name="elements">The JSON elements targeted by the index, one per indexed property.</param>
    /// <param name="collectionIndices">
    ///     The complex-collection indices traversed to reach each indexed property, parallel to
    ///     <paramref name="elements" />.
    /// </param>
    /// <param name="isDescending">
    ///     The sort order requested for each indexed JSON element, parallel to <paramref name="elements" />,
    ///     or <see langword="null" /> if every element uses ascending order.
    /// </param>
    public RelationalJsonIndex(
        IReadOnlyList<IRelationalJsonElement> elements,
        IReadOnlyList<IReadOnlyList<int?>?>? collectionIndices,
        IReadOnlyList<bool>? isDescending = null)
    {
        Check.NotNull(elements);

        if (collectionIndices is not null && elements.Count != collectionIndices.Count)
        {
            throw new ArgumentException(
                RelationalStrings.JsonPathIndexElementsCollectionIndicesMismatch(elements.Count, collectionIndices.Count),
                nameof(collectionIndices));
        }

        if (isDescending is not null && isDescending.Count != 0 && elements.Count != isDescending.Count)
        {
            throw new ArgumentException(
                RelationalStrings.JsonPathIndexElementsIsDescendingMismatch(elements.Count, isDescending.Count),
                nameof(isDescending));
        }

        Elements = elements;
        CollectionIndices = collectionIndices;
        IsDescending = isDescending;
    }

    /// <summary>
    ///     Gets the JSON elements targeted by the index, one per indexed property.
    /// </summary>
    public IReadOnlyList<IRelationalJsonElement> Elements { get; }

    /// <summary>
    ///     Gets the complex-collection indices traversed to reach each indexed property.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<int?>?>? CollectionIndices { get; }

    /// <summary>
    ///     Gets the sort order requested for each indexed JSON element, parallel to <see cref="Elements" />,
    ///     or <see langword="null" /> if every element uses ascending order.
    /// </summary>
    public IReadOnlyList<bool>? IsDescending { get; }

    /// <summary>
    ///     Resolves whether the element at <paramref name="index" /> should be sorted in descending order,
    ///     honoring the shorthand where an empty <see cref="IsDescending" /> list means every element is
    ///     descending.
    /// </summary>
    /// <param name="index">The index of the element in <see cref="Elements" />.</param>
    /// <returns><see langword="true" /> if the element should be sorted in descending order.</returns>
    public bool IsElementDescending(int index)
        => IsDescending is not null && (IsDescending.Count == 0 || IsDescending[index]);

    /// <inheritdoc />
    public bool Equals(RelationalJsonIndex? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null
            || Elements.Count != other.Elements.Count
            || (CollectionIndices is null) != (other.CollectionIndices is null)
            || (IsDescending is null) != (other.IsDescending is null))
        {
            return false;
        }

        // CollectionIndices, when non-null, has the same Count as Elements (enforced by the constructor).
        for (var i = 0; i < Elements.Count; i++)
        {
            if (!JsonElementsEqual(Elements[i], other.Elements[i]))
            {
                return false;
            }

            if (!CollectionIndicesEntryEqual(CollectionIndices?[i], other.CollectionIndices?[i]))
            {
                return false;
            }

            if (IsDescending is not null && IsElementDescending(i) != other.IsElementDescending(i))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => Equals(obj as RelationalJsonIndex);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        for (var i = 0; i < Elements.Count; i++)
        {
            var element = Elements[i];
            hash.Add(element.ContainingColumn.Name);
            foreach (var segment in element.Path)
            {
                hash.Add(segment.IsArray);
                hash.Add(segment.PropertyName);
            }

            var indices = CollectionIndices?[i];
            if (indices is null)
            {
                hash.Add(-1);
            }
            else
            {
                foreach (var entry in indices)
                {
                    hash.Add(entry.HasValue ? entry.Value : -1);
                }
            }

            hash.Add(IsDescending is null ? (bool?)null : IsElementDescending(i));
        }

        return hash.ToHashCode();
    }

    private static bool JsonElementsEqual(IRelationalJsonElement? left, IRelationalJsonElement? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        if (!string.Equals(left.ContainingColumn.Name, right.ContainingColumn.Name, StringComparison.Ordinal))
        {
            return false;
        }

        if (left.Path.Count != right.Path.Count)
        {
            return false;
        }

        for (var i = 0; i < left.Path.Count; i++)
        {
            var leftSegment = left.Path[i];
            var rightSegment = right.Path[i];
            if (leftSegment.IsArray != rightSegment.IsArray
                || !string.Equals(leftSegment.PropertyName, rightSegment.PropertyName, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static bool CollectionIndicesEntryEqual(IReadOnlyList<int?>? left, IReadOnlyList<int?>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null || left.Count != right.Count)
        {
            return false;
        }

        for (var i = 0; i < left.Count; i++)
        {
            if (left[i] != right[i])
            {
                return false;
            }
        }

        return true;
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     A <see cref="ValueComparer{T}" /> for lists of primitive items. The list can be typed as <see cref="IEnumerable{T}" />,
///     but can only be used with instances that implement <see cref="IList{T}" />.
/// </summary>
/// <remarks>
///     <para>
///         This comparer should be used when the element of the comparer is typed as <see cref="object" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-value-comparers">EF Core value comparers</see> for more information and examples.
///     </para>
/// </remarks>
/// <typeparam name="TElement">The element type.</typeparam>
public sealed class ObjectListComparer<TElement> : ValueComparer<IEnumerable<TElement>>
{
    /// <summary>
    ///     Creates a new instance of the list comparer.
    /// </summary>
    /// <param name="elementComparer">The comparer to use for comparing elements.</param>
    public ObjectListComparer(ValueComparer elementComparer)
        : base(
            (a, b) => Compare(a, b, elementComparer),
            o => GetHashCode(o, elementComparer),
            source => Snapshot(source, elementComparer))
    {
        ElementComparer = elementComparer;
    }

    /// <summary>
    ///     The comparer to use for comparing elements.
    /// </summary>
    public ValueComparer ElementComparer { get; }

    private static bool Compare(IEnumerable<TElement>? a, IEnumerable<TElement>? b, ValueComparer elementComparer)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null)
        {
            return b is null;
        }

        if (b is null)
        {
            return false;
        }

        if (a is IList<object?> aList && b is IList<object?> bList)
        {
            if (aList.Count != bList.Count)
            {
                return false;
            }

            for (var i = 0; i < aList.Count; i++)
            {
                var (el1, el2) = (aList[i], bList[i]);
                if (el1 is null)
                {
                    if (el2 is null)
                    {
                        continue;
                    }

                    return false;
                }

                if (el2 is null)
                {
                    return false;
                }

                if (!elementComparer.Equals(el1, el2))
                {
                    return false;
                }
            }

            return true;
        }

        throw new InvalidOperationException(
            CoreStrings.BadListType(
                (a is IList<TElement?> ? b : a).GetType().ShortDisplayName(),
                typeof(IList<>).MakeGenericType(elementComparer.Type).ShortDisplayName()));
    }

    private static int GetHashCode(IEnumerable<TElement> source, ValueComparer elementComparer)
    {
        var hash = new HashCode();

        foreach (var el in source)
        {
            hash.Add(el == null ? 0 : elementComparer.GetHashCode(el));
        }

        return hash.ToHashCode();
    }

    private static IList<TElement> Snapshot(IEnumerable<TElement> source, ValueComparer elementComparer)
    {
        if (source is not IList<TElement> sourceList)
        {
            throw new InvalidOperationException(
                CoreStrings.BadListType(
                    source.GetType().ShortDisplayName(),
                    typeof(IList<>).MakeGenericType(elementComparer.Type).ShortDisplayName()));
        }

        if (sourceList.IsReadOnly)
        {
            var snapshot = new TElement[sourceList.Count];

            for (var i = 0; i < sourceList.Count; i++)
            {
                var instance = sourceList[i];
                if (instance != null)
                {
                    snapshot[i] = (TElement)elementComparer.Snapshot(instance);
                }
            }

            return snapshot;
        }
        else
        {
            var snapshot = (source is List<TElement> || sourceList.IsReadOnly)
                ? new List<TElement>(sourceList.Count)
                : (IList<TElement>)Activator.CreateInstance(source.GetType())!;

            foreach (var e in sourceList)
            {
                snapshot.Add(e == null ? (TElement)(object?)null! : (TElement)elementComparer.Snapshot(e));
            }

            return snapshot;
        }
    }
}

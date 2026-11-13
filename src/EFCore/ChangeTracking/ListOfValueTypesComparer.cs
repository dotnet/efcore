// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     A <see cref="ValueComparer{T}" /> for lists of primitive items. The list can be typed as <see cref="IEnumerable{T}" />,
///     but can only be used with instances that implement <see cref="IList{T}" />.
/// </summary>
/// <remarks>
///     <para>
///         This comparer should be used for reference types and non-nullable value types. Use
///         <see cref="ListOfNullableValueTypesComparer{TConcreteList,TElement}" /> for nullable value types.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-value-comparers">EF Core value comparers</see> for more information and examples.
///     </para>
/// </remarks>
/// <typeparam name="TConcreteList">The collection type to create an index of, if needed.</typeparam>
/// <typeparam name="TElement">The element type.</typeparam>
public sealed class ListOfValueTypesComparer<TConcreteList, TElement> : ValueComparer<IEnumerable<TElement>>, IInfrastructure<ValueComparer>
    where TElement : struct
{
    private static readonly bool IsArray = typeof(TConcreteList).IsArray;

    private static readonly bool IsReadOnly = IsArray
        || (typeof(TConcreteList).IsGenericType
            && typeof(TConcreteList).GetGenericTypeDefinition() == typeof(ReadOnlyCollection<>));

    private static readonly MethodInfo CompareMethod = typeof(ListOfValueTypesComparer<TConcreteList, TElement>).GetMethod(
        nameof(Compare), BindingFlags.Static | BindingFlags.NonPublic,
        [typeof(IEnumerable<TElement>), typeof(IEnumerable<TElement>), typeof(Func<TElement, TElement, bool>)])!;

    private static readonly MethodInfo GetHashCodeMethod = typeof(ListOfValueTypesComparer<TConcreteList, TElement>).GetMethod(
        nameof(GetHashCode), BindingFlags.Static | BindingFlags.NonPublic,
        [typeof(IEnumerable<TElement>), typeof(Func<TElement, int>)])!;

    private static readonly MethodInfo SnapshotMethod = typeof(ListOfValueTypesComparer<TConcreteList, TElement>).GetMethod(
        nameof(Snapshot), BindingFlags.Static | BindingFlags.NonPublic, [typeof(IEnumerable<TElement>), typeof(Func<TElement, TElement>)])!;

    /// <summary>
    ///     Creates a new instance of the list comparer.
    /// </summary>
    /// <param name="elementComparer">The comparer to use for comparing elements.</param>
    public ListOfValueTypesComparer(ValueComparer elementComparer)
        : base(
            CompareLambda(elementComparer),
            GetHashCodeLambda(elementComparer),
            SnapshotLambda(elementComparer))
        => ElementComparer = elementComparer;

    /// <summary>
    ///     The comparer to use for comparing elements.
    /// </summary>
    public ValueComparer ElementComparer { get; }

    ValueComparer IInfrastructure<ValueComparer>.Instance
        => ElementComparer;

    private static Expression<Func<IEnumerable<TElement>?, IEnumerable<TElement>?, bool>> CompareLambda(ValueComparer elementComparer)
    {
        var prm1 = Expression.Parameter(typeof(IEnumerable<TElement>), "a");
        var prm2 = Expression.Parameter(typeof(IEnumerable<TElement>), "b");

        //(a, b) => Compare(a, b, elementComparer.Equals)
        return Expression.Lambda<Func<IEnumerable<TElement>?, IEnumerable<TElement>?, bool>>(
            Expression.Call(
                CompareMethod,
                prm1,
                prm2,
                elementComparer.EqualsExpression),
            prm1,
            prm2);
    }

    private static Expression<Func<IEnumerable<TElement>, int>> GetHashCodeLambda(ValueComparer elementComparer)
    {
        var prm = Expression.Parameter(typeof(IEnumerable<TElement>), "o");

        //o => GetHashCode(o, elementComparer.GetHashCode)
        return Expression.Lambda<Func<IEnumerable<TElement>, int>>(
            Expression.Call(
                GetHashCodeMethod,
                prm,
                elementComparer.HashCodeExpression),
            prm);
    }

    private static Expression<Func<IEnumerable<TElement>, IEnumerable<TElement>>> SnapshotLambda(ValueComparer elementComparer)
    {
        var prm = Expression.Parameter(typeof(IEnumerable<TElement>), "source");

        //source => Snapshot(source, elementComparer.Snapshot)
        return Expression.Lambda<Func<IEnumerable<TElement>, IEnumerable<TElement>>>(
            Expression.Call(
                SnapshotMethod,
                prm,
                elementComparer.SnapshotExpression),
            prm);
    }

    private static bool Compare(IEnumerable<TElement>? a, IEnumerable<TElement>? b, Func<TElement, TElement, bool> elementCompare)
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

        if (a is IList<TElement> aList && b is IList<TElement> bList)
        {
            if (aList.Count != bList.Count)
            {
                return false;
            }

            for (var i = 0; i < aList.Count; i++)
            {
                var (el1, el2) = (aList[i], bList[i]);
                if (!elementCompare(el1, el2))
                {
                    return false;
                }
            }

            return true;
        }

        throw new InvalidOperationException(
            CoreStrings.BadListType(
                (a is IList<TElement?> ? b : a).GetType().ShortDisplayName(),
                typeof(IList<>).MakeGenericType(typeof(TElement)).ShortDisplayName()));
    }

    private static int GetHashCode(IEnumerable<TElement> source, Func<TElement, int> elementGetHashCode)
    {
        var hash = new HashCode();

        foreach (var el in source)
        {
            hash.Add(elementGetHashCode(el));
        }

        return hash.ToHashCode();
    }

    private static IList<TElement> Snapshot(IEnumerable<TElement> source, Func<TElement, TElement> elementSnapshot)
    {
        if (source is not IList<TElement> sourceList)
        {
            throw new InvalidOperationException(
                CoreStrings.BadListType(
                    source.GetType().ShortDisplayName(),
                    typeof(IList<>).MakeGenericType(typeof(TElement).MakeNullable()).ShortDisplayName()));
        }

        if (IsArray)
        {
            var snapshot = new TElement[sourceList.Count];
            for (var i = 0; i < sourceList.Count; i++)
            {
                var instance = sourceList[i];
                snapshot[i] = elementSnapshot(instance);
            }

            return snapshot;
        }
        else
        {
            var snapshot = IsReadOnly ? new List<TElement>() : (IList<TElement>)Activator.CreateInstance<TConcreteList>()!;
            foreach (var e in sourceList)
            {
                snapshot.Add(elementSnapshot(e));
            }

            return IsReadOnly
                ? (IList<TElement>)Activator.CreateInstance(typeof(TConcreteList), snapshot)!
                : snapshot;
        }
    }
}

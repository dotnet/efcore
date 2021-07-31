// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore.Cosmos.ChangeTracking.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public sealed class NullableStringDictionaryComparer<TElement, TCollection> : ValueComparer<TCollection>
        where TCollection : class, IEnumerable<KeyValuePair<string, TElement?>>
        where TElement : struct
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NullableStringDictionaryComparer(ValueComparer elementComparer, bool readOnly)
            : base(
                (a, b) => Compare(a, b, (ValueComparer<TElement>)elementComparer),
                o => GetHashCode(o, (ValueComparer<TElement>)elementComparer),
                source => Snapshot(source, (ValueComparer<TElement>)elementComparer, readOnly))
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Type Type => typeof(TCollection);

        private static bool Compare(TCollection? a, TCollection? b, ValueComparer<TElement> elementComparer)
        {
            if (a is not IReadOnlyDictionary<string, TElement?> aDict)
            {
                return b is not IReadOnlyDictionary<string, TElement?>;
            }

            if (b is not IReadOnlyDictionary<string, TElement?> bDict || aDict.Count != bDict.Count)
            {
                return false;
            }

            if (ReferenceEquals(aDict, bDict))
            {
                return true;
            }

            foreach (var aPair in aDict)
            {
                if (!bDict.TryGetValue(aPair.Key, out var bValue))
                {
                    return false;
                }

                if (aPair.Value is null)
                {
                    if (bValue is null)
                    {
                        continue;
                    }

                    return false;
                }

                if (bValue is null || !elementComparer.Equals(aPair.Value, bValue))
                {
                    return false;
                }
            }

            return true;
        }

        private static int GetHashCode(TCollection source, ValueComparer<TElement> elementComparer)
        {
            var nullableEqualityComparer = new NullableEqualityComparer<TElement>(elementComparer);
            var hash = new HashCode();
            foreach (var el in source)
            {
                hash.Add(el.Key);
                hash.Add(el.Value, nullableEqualityComparer);
            }

            return hash.ToHashCode();
        }

        private static TCollection? Snapshot(TCollection? source, ValueComparer<TElement> elementComparer, bool readOnly)
        {
            if (source is null)
            {
                return null;
            }

            if (readOnly)
            {
                return source;
            }

            var snapshot = new Dictionary<string, TElement?>(((IReadOnlyDictionary<string, TElement?>)source).Count);
            foreach (var e in source)
            {
                snapshot.Add(e.Key, e.Value is null ? null : elementComparer.Snapshot(e.Value.Value));
            }

            return (TCollection)(object)snapshot;
        }
    }
}

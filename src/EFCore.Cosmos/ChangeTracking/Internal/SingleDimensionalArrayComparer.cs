// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore.Cosmos.ChangeTracking.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public sealed class SingleDimensionalArrayComparer<TElement> : ValueComparer<TElement[]>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SingleDimensionalArrayComparer(ValueComparer elementComparer) : base(
            (a, b) => Compare(a, b, (ValueComparer<TElement>)elementComparer),
            o => GetHashCode(o, (ValueComparer<TElement>)elementComparer),
            source => Snapshot(source, (ValueComparer<TElement>)elementComparer))
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Type Type => typeof(TElement[]);

        private static bool Compare(TElement[]? a, TElement[]? b, ValueComparer<TElement> elementComparer)
        {
            if (a is null)
            {
                return b is null;
            }

            if (b is null || a.Length != b.Length)
            {
                return false;
            }

            if (ReferenceEquals(a, b))
            {
                return true;
            }

            for (var i = 0; i < a.Length; i++)
            {
                if (!elementComparer.Equals(a[i], b[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static int GetHashCode(TElement[] source, ValueComparer<TElement> elementComparer)
        {
            var hash = new HashCode();
            foreach (var el in source)
            {
                hash.Add(el, elementComparer);
            }

            return hash.ToHashCode();
        }

        [return: NotNullIfNotNull("source")]
        private static TElement[]? Snapshot(TElement[]? source, ValueComparer<TElement> elementComparer)
        {
            if (source is null)
            {
                return null;
            }

            var snapshot = new TElement[source.Length];
            for (var i = 0; i < source.Length; i++)
            {
                snapshot[i] = elementComparer.Snapshot(source[i])!;
            }
            return snapshot;
        }
    }
}

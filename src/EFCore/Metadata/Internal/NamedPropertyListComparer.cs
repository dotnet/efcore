// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // Sealed for perf
    public sealed class NamedPropertyListComparer : IComparer<(IReadOnlyList<IProperty>, string)>, IEqualityComparer<(IReadOnlyList<IProperty>, string)>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly NamedPropertyListComparer Instance = new NamedPropertyListComparer();

        private NamedPropertyListComparer()
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public int Compare((IReadOnlyList<IProperty>, string) x, (IReadOnlyList<IProperty>, string) y)
        {
            var result = x.Item1.Count - y.Item1.Count;

            if (result != 0)
            {
                return result;
            }

            var index = 0;
            while ((result == 0)
                && (index < x.Item1.Count))
            {
                result = StringComparer.Ordinal.Compare(x.Item1[index].Name, y.Item1[index].Name);
                index++;
            }

            if (result != 0)
            {
                return result;
            }

            return string.Compare(x.Item2, y.Item2, StringComparison.Ordinal);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public bool Equals((IReadOnlyList<IProperty>, string) x, (IReadOnlyList<IProperty>, string) y)
            => Compare(x, y) == 0;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public int GetHashCode((IReadOnlyList<IProperty>, string) obj)
        {
            var hash = new HashCode();
            for (var i = 0; i < obj.Item1.Count; i++)
            {
                hash.Add(obj.Item1[i]);
            }

            hash.Add(obj.Item2);

            return hash.ToHashCode();
        }
    }
}

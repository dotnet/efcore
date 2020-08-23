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
    public sealed class NamedListComparer : IComparer<(string, string, IReadOnlyList<string>)>,
        IEqualityComparer<(string, string, IReadOnlyList<string>)>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly NamedListComparer Instance = new NamedListComparer();

        private NamedListComparer()
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public int Compare((string, string, IReadOnlyList<string>) x, (string, string, IReadOnlyList<string>) y)
        {
            var result = StringComparer.Ordinal.Compare(x.Item1, y.Item1);
            if (result != 0)
            {
                return result;
            }

            result = StringComparer.Ordinal.Compare(x.Item2, y.Item2);
            if (result != 0)
            {
                return result;
            }

            result = x.Item3.Count - y.Item3.Count;
            if (result != 0)
            {
                return result;
            }

            var index = 0;
            while ((result == 0)
                && (index < x.Item3.Count))
            {
                result = StringComparer.Ordinal.Compare(x.Item3[index], y.Item3[index]);
                index++;
            }

            return result;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public bool Equals((string, string, IReadOnlyList<string>) x, (string, string, IReadOnlyList<string>) y)
            => Compare(x, y) == 0;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public int GetHashCode((string, string, IReadOnlyList<string>) obj)
        {
            var hash = new HashCode();
            hash.Add(obj.Item1);
            hash.Add(obj.Item2);
            for (var i = 0; i < obj.Item3.Count; i++)
            {
                hash.Add(obj.Item3[i]);
            }

            return hash.ToHashCode();
        }
    }
}

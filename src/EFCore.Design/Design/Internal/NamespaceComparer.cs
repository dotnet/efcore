// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    /// <summary>
    ///     A custom string comparer to sort using statements to have System prefixed namespaces first.
    /// </summary>
    public class NamespaceComparer : IComparer<string>
    {
        /// <inheritdoc />
        public virtual int Compare(string x, string y)
        {
            var xSystemNamespace = x != null && (string.Equals(x, "System") || x.StartsWith("System."));
            var ySystemNamespace = y != null && (string.Equals(y, "System") || y.StartsWith("System."));

            return xSystemNamespace && !ySystemNamespace
                ? -1
                : !xSystemNamespace && ySystemNamespace
                    ? 1
                    : string.CompareOrdinal(x, y);
        }
    }
}

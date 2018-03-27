// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SemanticVersionComparer : IComparer<string>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual int Compare(string x, string y)
            => CreateVersion(x).CompareTo(CreateVersion(y));

        private static Version CreateVersion(string semanticVersion)
        {
            var prereleaseIndex = semanticVersion.IndexOf("-", StringComparison.Ordinal);
            if (prereleaseIndex != -1)
            {
                semanticVersion = semanticVersion.Substring(0, prereleaseIndex);
            }

            return new Version(semanticVersion);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.XuGu.Utilities
{
    internal static class Statics
    {
        internal static readonly bool[][] TrueArrays =
        {
            Array.Empty<bool>(),
            new[] { true },
            new[] { true, true },
            new[] { true, true, true },
            new[] { true, true, true, true },
            new[] { true, true, true, true, true },
            new[] { true, true, true, true, true, true },
            new[] { true, true, true, true, true, true, true },
            new[] { true, true, true, true, true, true, true, true },
            new[] { true, true, true, true, true, true, true, true, true },
            new[] { true, true, true, true, true, true, true, true, true, true },
            new[] { true, true, true, true, true, true, true, true, true, true, true },
            new[] { true, true, true, true, true, true, true, true, true, true, true, true },
            new[] { true, true, true, true, true, true, true, true, true, true, true, true, true },
            new[] { true, true, true, true, true, true, true, true, true, true, true, true, true, true },
            new[] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
            new[] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
        };

        internal static readonly bool[][] FalseArrays =
        {
            Array.Empty<bool>(),
            new[] { false },
            new[] { false, false },
            new[] { false, false, false },
            new[] { false, false, false, false },
            new[] { false, false, false, false, false },
            new[] { false, false, false, false, false, false },
            new[] { false, false, false, false, false, false, false },
            new[] { false, false, false, false, false, false, false, false },
            new[] { false, false, false, false, false, false, false, false, false },
            new[] { false, false, false, false, false, false, false, false, false, false },
            new[] { false, false, false, false, false, false, false, false, false, false, false },
            new[] { false, false, false, false, false, false, false, false, false, false, false, false },
            new[] { false, false, false, false, false, false, false, false, false, false, false, false, false },
            new[] { false, false, false, false, false, false, false, false, false, false, false, false, false, false },
            new[] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
            new[] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
        };

        internal static IEnumerable<bool> GetTrueValues(int dimensions)
            => dimensions <= 16
                ? TrueArrays[dimensions]
                : Enumerable.Repeat(true, dimensions);

        internal static IEnumerable<bool> GetFalseValues(int dimensions)
            => dimensions <= 16
                ? FalseArrays[dimensions]
                : Enumerable.Repeat(true, dimensions);
    }
}

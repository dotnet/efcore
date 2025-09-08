// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.XuGu.Extensions
{
    internal static class StringExtensions
    {
        internal static string NullIfEmpty(this string value)
            => value?.Length > 0
                ? value
                : null;
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Options to print debug string differently for metadata objects.
    /// </summary>
    [Flags]
    public enum MetadataDebugStringOptions
    {
        /// <summary>
        ///     Include annotations in debug string.
        /// </summary>
        IncludeAnnotations = 1,

        /// <summary>
        ///     Include property indexes in debug string.
        /// </summary>
        IncludePropertyIndexes = 2,

        /// <summary>
        ///     Print debug string on single line only.
        /// </summary>
        SingleLine = 4,

        /// <summary>
        ///     Default settings for short debug string.
        /// </summary>
        ShortDefault = 0,

        /// <summary>
        ///     Default settings for long debug string.
        /// </summary>
        LongDefault = IncludeAnnotations,

        /// <summary>
        ///     Default settings for single line debug string.
        /// </summary>
        SingleLineDefault = SingleLine
    }
}

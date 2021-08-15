// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     Debug string customization options for tracked entities.
    /// </summary>
    /// <seealso href="https://aka.ms/efcore-docs-debug-views">Documentation for EF Core debug views.</seealso>
    /// <seealso href="https://aka.ms/efcore-docs-change-tracking">Documentation for EF Core change tracking.</seealso>
    [Flags]
    public enum ChangeTrackerDebugStringOptions
    {
        /// <summary>
        ///     Include non-navigation properties in debug string.
        /// </summary>
        IncludeProperties = 1,

        /// <summary>
        ///     Include navigation properties in debug string.
        /// </summary>
        IncludeNavigations = 2,

        /// <summary>
        ///     Default settings for short debug string.
        /// </summary>
        ShortDefault = 0,

        /// <summary>
        ///     Default settings for long debug string.
        /// </summary>
        LongDefault = IncludeProperties | IncludeNavigations,
    }
}

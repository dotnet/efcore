// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     Debug string customization options for tracked entities.
    /// </summary>
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

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion
{
    public enum NavigationState
    {
        /// <summary>
        ///     Navigation doesn't need to be processed
        /// </summary>
        NotNeeded,

        /// <summary>
        ///     Navigation needs to be processed, but hasn't been processed yet
        /// </summary>
        Pending,

        /// <summary>
        ///     Navigation needs to be processed, but after navigation expansion
        /// </summary>
        Delayed,

        /// <summary>
        ///     Navigation has already been processed
        /// </summary>
        Complete,
    };
}

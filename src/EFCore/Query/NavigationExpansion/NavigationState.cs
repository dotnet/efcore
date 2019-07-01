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
        ///     Reference navigation needs to be processed, but hasn't been processed yet
        /// </summary>
        ReferencePending,

        /// <summary>
        ///     Reference navigation has already been processed
        /// </summary>
        ReferenceComplete,

        /// <summary>
        ///     Collection navigation needs to be processed, but hasn't been processed yet
        /// </summary>
        CollectionPending,

        /// <summary>
        ///     Collection navigation has already been processed
        /// </summary>
        CollectionComplete,
    };
}

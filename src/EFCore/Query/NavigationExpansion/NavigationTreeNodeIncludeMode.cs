// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion
{
    public enum NavigationTreeNodeIncludeMode
    {
        /// <summary>
        ///     Navigation doesn't need to be included
        /// </summary>
        NotNeeded,

        /// <summary>
        ///     Navigation needs to be included, but hasn't been included yet
        /// </summary>
        ReferencePending,

        /// <summary>
        ///     Navigation has already been included
        /// </summary>
        ReferenceComplete,

        /// <summary>
        ///     Collection navigation needs to be included
        /// </summary>
        Collection,
    };
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion
{
    public enum NavigationTreeNodeExpansionMode
    {
        /// <summary>
        ///     Navigation doesn't need to be expanded
        /// </summary>
        NotNeeded,

        /// <summary>
        ///     Reference navigation needs to be expanded, but hasn't been expanded yet
        /// </summary>
        ReferencePending,

        /// <summary>
        ///     Reference navigation has already been expanded
        /// </summary>
        ReferenceComplete,

        /// <summary>
        ///     Collection navigation needs to be expanded
        /// </summary>
        Collection,
    };
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A common interface for event payload classes that have an <see cref="INavigationBase" />.
    /// </summary>
    public interface INavigationBaseEventData
    {
        /// <summary>
        ///     The navigation.
        /// </summary>
        INavigationBase NavigationBase { get; }
    }
}

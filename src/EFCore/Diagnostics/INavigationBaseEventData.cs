﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

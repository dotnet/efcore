// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A common interface for event payload classes that have an <see cref="INavigationBase" />.
    /// </summary>
    /// <remarks>
    ///     For more information, see <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see>.
    /// </remarks>
    public interface INavigationBaseEventData
    {
        /// <summary>
        ///     The navigation.
        /// </summary>
        INavigationBase NavigationBase { get; }
    }
}

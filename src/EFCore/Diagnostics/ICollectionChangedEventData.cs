// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     An interface implemented by any <see cref="EventData" /> subclass that represents a change to either
///     a skip collection navigation or a regular collection navigation.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public interface ICollectionChangedEventData
{
    /// <summary>
    ///     The entry for the entity instance on which the navigation property has been added
    ///     to or removed from.
    /// </summary>
    EntityEntry EntityEntry { get; }

    /// <summary>
    ///     The entities added to the collection.
    /// </summary>
    IEnumerable<object> Added { get; }

    /// <summary>
    ///     The entities removed from the collection.
    /// </summary>
    IEnumerable<object> Removed { get; }
}

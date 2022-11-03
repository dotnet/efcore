// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Event arguments for the <see cref="DbContext.SavedChanges" /> event.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-saving-data">Saving data in EF Core</see> and
///     <see href="https://aka.ms/efcore-docs-events">EF Core events</see> for more information and examples.
/// </remarks>
public class SavedChangesEventArgs : SaveChangesEventArgs
{
    /// <summary>
    ///     Creates a new <see cref="SavedChangesEventArgs" /> instance with the given number of entities saved.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">The value passed to SaveChanges.</param>
    /// <param name="entitiesSavedCount">The number of entities saved.</param>
    public SavedChangesEventArgs(bool acceptAllChangesOnSuccess, int entitiesSavedCount)
        : base(acceptAllChangesOnSuccess)
    {
        EntitiesSavedCount = entitiesSavedCount;
    }

    /// <summary>
    ///     The number of entities saved.
    /// </summary>
    public virtual int EntitiesSavedCount { get; }
}

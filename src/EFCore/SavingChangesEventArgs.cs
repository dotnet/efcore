// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Event arguments for the <see cref="DbContext.SavingChanges" /> event.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-saving-data">Saving data in EF Core</see> and
///     <see href="https://aka.ms/efcore-docs-events">EF Core events</see> for more information and examples.
/// </remarks>
public class SavingChangesEventArgs : SaveChangesEventArgs
{
    /// <summary>
    ///     Creates event arguments for the <see cref="O:DbContext.SavingChanges" /> event.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">The value passed to SaveChanges.</param>
    public SavingChangesEventArgs(bool acceptAllChangesOnSuccess)
        : base(acceptAllChangesOnSuccess)
    {
    }
}

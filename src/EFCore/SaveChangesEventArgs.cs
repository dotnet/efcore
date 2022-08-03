// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Base event arguments for the <see cref="O:DbContext.SaveChanges" /> and <see cref="O:DbContext.SaveChangesAsync" /> events.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-saving-data">Saving data in EF Core</see> and
///     <see href="https://aka.ms/efcore-docs-events">EF Core events</see> for more information and examples.
/// </remarks>
public abstract class SaveChangesEventArgs : EventArgs
{
    /// <summary>
    ///     Creates a base event arguments instance for <see cref="O:DbContext.SaveChanges" />
    ///     or <see cref="O:DbContext.SaveChangesAsync" /> events.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">The value passed to SaveChanges.</param>
    protected SaveChangesEventArgs(bool acceptAllChangesOnSuccess)
    {
        AcceptAllChangesOnSuccess = acceptAllChangesOnSuccess;
    }

    /// <summary>
    ///     The value passed to <see cref="O:DbContext.SaveChanges" /> or <see cref="O:DbContext.SaveChangesAsync" />.
    /// </summary>
    public virtual bool AcceptAllChangesOnSuccess { get; }
}

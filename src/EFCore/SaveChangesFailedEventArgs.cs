// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Event arguments for the <see cref="DbContext.SaveChangesFailed" /> event.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-saving-data">Saving data in EF Core</see> and
///     <see href="https://aka.ms/efcore-docs-events">EF Core events</see> for more information and examples.
/// </remarks>
public class SaveChangesFailedEventArgs : SaveChangesEventArgs
{
    /// <summary>
    ///     Creates a new <see cref="SaveChangesFailedEventArgs" /> instance with the exception that was thrown.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">The value passed to SaveChanges.</param>
    /// <param name="exception">The exception thrown.</param>
    public SaveChangesFailedEventArgs(bool acceptAllChangesOnSuccess, Exception exception)
        : base(acceptAllChangesOnSuccess)
    {
        Exception = exception;
    }

    /// <summary>
    ///     The exception thrown during<see cref="O:DbContext.SaveChanges" /> or <see cref="O:DbContext.SaveChangesAsync" />.
    /// </summary>
    public virtual Exception Exception { get; }
}

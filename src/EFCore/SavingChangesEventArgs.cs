// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Event arguments for the <see cref="DbContext.SavingChanges" /> event.
    /// </summary>
    public class SavingChangesEventArgs : SaveChangesEventArgs
    {
        /// <summary>
        ///     Creates event arguments for the <see cref="M:DbContext.SavingChanges" /> event.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess"> The value passed to SaveChanges. </param>
        public SavingChangesEventArgs(bool acceptAllChangesOnSuccess)
            : base(acceptAllChangesOnSuccess)
        {
        }
    }
}

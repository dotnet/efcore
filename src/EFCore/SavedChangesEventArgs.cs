// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Event arguments for the <see cref="DbContext.SavedChanges" /> event.
    /// </summary>
    public class SavedChangesEventArgs : SaveChangesEventArgs
    {
        /// <summary>
        ///     Creates a new <see cref="SavedChangesEventArgs" /> instance with the given number of entities saved.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess"> The value passed to SaveChanges. </param>
        /// <param name="entitiesSavedCount"> The number of entities saved. </param>
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
}

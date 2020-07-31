// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

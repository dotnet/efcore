// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

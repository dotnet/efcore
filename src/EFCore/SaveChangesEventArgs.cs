﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Base event arguments for the <see cref="M:DbContext.SaveChanges" /> and <see cref="M:DbContext.SaveChangesAsync" /> events.
    /// </summary>
    public abstract class SaveChangesEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates a base event arguments instance for <see cref="M:DbContext.SaveChanges" />
        ///     or <see cref="M:DbContext.SaveChangesAsync" /> events.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess"> The value passed to SaveChanges. </param>
        protected SaveChangesEventArgs(bool acceptAllChangesOnSuccess)
        {
            AcceptAllChangesOnSuccess = acceptAllChangesOnSuccess;
        }

        /// <summary>
        ///     The value passed to <see cref="M:DbContext.SaveChanges" /> or <see cref="M:DbContext.SaveChangesAsync" />.
        /// </summary>
        public virtual bool AcceptAllChangesOnSuccess { get; }
    }
}

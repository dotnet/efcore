﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Event arguments for the <see cref="DbContext.SaveChangesFailed" /> event.
    /// </summary>
    public class SaveChangesFailedEventArgs : SaveChangesEventArgs
    {
        /// <summary>
        ///     Creates a new <see cref="SaveChangesFailedEventArgs" /> instance with the exception that was thrown.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess"> The value passed to SaveChanges. </param>
        /// <param name="exception"> The exception thrown. </param>
        public SaveChangesFailedEventArgs(bool acceptAllChangesOnSuccess, Exception exception)
            : base(acceptAllChangesOnSuccess)
        {
            Exception = exception;
        }

        /// <summary>
        ///     The exception thrown during<see cref="M:DbContext.SaveChanges" /> or <see cref="M:DbContext.SaveChangesAsync" />.
        /// </summary>
        public virtual Exception Exception { get; }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

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
        public SaveChangesFailedEventArgs(bool acceptAllChangesOnSuccess, [NotNull] Exception exception)
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

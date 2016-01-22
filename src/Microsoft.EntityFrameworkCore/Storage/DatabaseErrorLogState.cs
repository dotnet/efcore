// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Provided as the state to <see cref="ILogger" /> when logging an exception that occurred while accessing the database.
    /// </summary>
    public class DatabaseErrorLogState
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DatabaseErrorLogState" /> class.
        /// </summary>
        /// <param name="contextType"> The type of the derived context that the error occurred for. </param>
        public DatabaseErrorLogState([NotNull] Type contextType)
        {
            Check.NotNull(contextType, nameof(contextType));

            ContextType = contextType;
        }

        /// <summary>
        ///     Gets the type of the derived context that the error occurred for.
        /// </summary>
        public virtual Type ContextType { get; }
    }
}

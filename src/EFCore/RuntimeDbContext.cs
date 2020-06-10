// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Lightweight version of <see cref="DbContext"/> used only during run-time.
    /// </summary>
    public class RuntimeDbContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RuntimeDbContext" /> class.
        /// </summary>
        public RuntimeDbContext()
        {
        }

        /// <summary>
        ///     Gets the compiled model
        /// </summary>
        /// <returns> The model </returns>
        public virtual IModel GetCompiledModel() => throw new InvalidOperationException("This should be overriden");
    }
}

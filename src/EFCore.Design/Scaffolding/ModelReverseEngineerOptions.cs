// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Represents the options to use while reverse engineering a model from the database.
    /// </summary>
    public class ModelReverseEngineerOptions
    {
        /// <summary>
        ///     Gets or sets a value indicating whether to use the database schema names directly.
        /// </summary>
        /// <value> A value indicating whether to use the database schema names directly. </value>
        public virtual bool UseDatabaseNames { get; set; }
    }
}

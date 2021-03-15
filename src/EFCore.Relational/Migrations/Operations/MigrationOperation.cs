// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     Base class for all Migrations operations that can be performed against a database.
    /// </summary>
    public abstract class MigrationOperation : Annotatable
    {
        /// <summary>
        ///     Indicates whether or not the operation might result in loss of data in the database.
        /// </summary>
        public virtual bool IsDestructiveChange { get; set; }
    }
}

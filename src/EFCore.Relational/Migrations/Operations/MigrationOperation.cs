// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

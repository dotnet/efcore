// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for dropping a schema.
    /// </summary>
    [DebuggerDisplay("DROP SCHEMA {Name}")]
    public class DropSchemaOperation : MigrationOperation
    {
        /// <summary>
        ///     The name of the schema.
        /// </summary>
        public virtual string Name { get; set; } = null!;
    }
}

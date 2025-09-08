// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for dropping a primary key.
    /// </summary>
    [DebuggerDisplay("ALTER TABLE {Table} DROP CONSTRAINT {Name}")]
    public class XGDropPrimaryKeyAndRecreateForeignKeysOperation : DropPrimaryKeyOperation
    {
        /// <summary>
        ///     Recreate all foreign keys or not.
        /// </summary>
        public virtual bool RecreateForeignKeys { get; set; }
    }
}

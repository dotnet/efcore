// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> to add a new column.
    /// </summary>
    [DebuggerDisplay("ALTER TABLE {Table} ADD {Name}")]
    public class AddColumnOperation : ColumnOperation
    {
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

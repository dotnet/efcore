// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;

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
        public virtual string Name { get; [param: NotNull] set; }
    }
}

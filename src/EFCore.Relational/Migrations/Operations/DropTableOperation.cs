// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for dropping an existing table.
    /// </summary>
    public class DropTableOperation : MigrationOperation
    {
        /// <summary>
        ///     Creates a new <see cref="DropTableOperation" />.
        /// </summary>
        // ReSharper disable once VirtualMemberCallInConstructor
        public DropTableOperation() => IsDestructiveChange = true;

        /// <summary>
        ///     The name of the table.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The schema that contains the table, or <c>null</c> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }
    }
}

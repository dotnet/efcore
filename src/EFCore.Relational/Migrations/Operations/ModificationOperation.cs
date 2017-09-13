// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for deleting, inserting or modifying seed data from an existing table.
    /// </summary>
    public abstract class ModificationOperation : MigrationOperation
    {
        private IEnumerable<ModificationCommand> _modificationCommands;

        /// <summary>
        ///     The commands that correspond to this operation.
        /// </summary>
        public virtual IEnumerable<ModificationCommand> ModificationCommands
            => _modificationCommands ?? (_modificationCommands = GenerateModificationCommands());

        /// <summary>
        ///     Generates the commands that correspond to this operation.
        /// </summary>
        /// <returns> The commands that correspond to this operation. </returns>
        protected abstract IEnumerable<ModificationCommand> GenerateModificationCommands();
    }
}

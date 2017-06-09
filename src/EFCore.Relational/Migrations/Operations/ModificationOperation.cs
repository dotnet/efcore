// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    public abstract class ModificationOperation : MigrationOperation
    {
        private IEnumerable<ModificationCommandBase> _modificationCommands;
        public virtual IEnumerable<ModificationCommandBase> ModificationCommands
        {
            get => _modificationCommands ?? (_modificationCommands = GenerateModificationCommands());
        }

        protected abstract IEnumerable<ModificationCommandBase> GenerateModificationCommands();
    }
}

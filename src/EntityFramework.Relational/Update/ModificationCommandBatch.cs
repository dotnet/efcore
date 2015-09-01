// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Update
{
    public abstract class ModificationCommandBatch
    {
        protected ModificationCommandBatch([NotNull] IUpdateSqlGenerator sqlGenerator)
        {
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));

            UpdateSqlGenerator = sqlGenerator;
        }

        protected virtual IUpdateSqlGenerator UpdateSqlGenerator { get; private set; }

        public abstract IReadOnlyList<ModificationCommand> ModificationCommands { get; }

        public abstract bool AddCommand([NotNull] ModificationCommand modificationCommand);

        public abstract void Execute([NotNull] IRelationalConnection connection);

        public abstract Task ExecuteAsync(
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}

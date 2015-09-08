// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Update
{
    public abstract class ModificationCommandBatch
    {
        protected ModificationCommandBatch([NotNull] IUpdateSqlGenerator sqlGenerator)
        {
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));

            UpdateSqlGenerator = sqlGenerator;
        }

        protected IUpdateSqlGenerator UpdateSqlGenerator { get; private set; }

        public abstract IReadOnlyList<ModificationCommand> ModificationCommands { get; }

        public abstract bool AddCommand([NotNull] ModificationCommand modificationCommand);

        public abstract void Execute(
            [NotNull] IRelationalTransaction transaction,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] DbContext context,
            [NotNull] ILogger logger);

        public abstract Task ExecuteAsync(
            [NotNull] IRelationalTransaction transaction,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] DbContext context,
            [NotNull] ILogger logger,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}

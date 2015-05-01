// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Update
{
    public abstract class ModificationCommandBatch
    {
        protected ModificationCommandBatch([NotNull] ISqlGenerator sqlGenerator)
        {
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));

            SqlGenerator = sqlGenerator;
        }

        protected ISqlGenerator SqlGenerator { get; private set; }

        public abstract IReadOnlyList<ModificationCommand> ModificationCommands { get; }

        public abstract bool AddCommand([NotNull] ModificationCommand modificationCommand);

        public abstract int Execute(
            [NotNull] RelationalTransaction transaction,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] DbContext context,
            [NotNull] ILogger logger);

        public abstract Task<int> ExecuteAsync(
            [NotNull] RelationalTransaction transaction,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] DbContext context,
            [NotNull] ILogger logger,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}

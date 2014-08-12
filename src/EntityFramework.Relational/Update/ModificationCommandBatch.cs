// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Update
{
    public abstract class ModificationCommandBatch
    {
        public abstract IReadOnlyList<ModificationCommand> ModificationCommands { get; }

        public abstract bool AddCommand(
            [NotNull] ModificationCommand modificationCommand,
            [NotNull] SqlGenerator sqlGenerator);

        public abstract Task<int> ExecuteAsync(
            [NotNull] RelationalTransaction transaction,
            [NotNull] RelationalTypeMapper typeMapper,
            [NotNull] DbContext context,
            [NotNull] ILogger logger,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}

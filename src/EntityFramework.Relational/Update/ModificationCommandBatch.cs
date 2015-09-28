// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Update
{
    public abstract class ModificationCommandBatch
    {
        public abstract IReadOnlyList<ModificationCommand> ModificationCommands { get; }

        public abstract bool AddCommand([NotNull] ModificationCommand modificationCommand);

        public abstract void Execute(
            [NotNull] IRelationalConnection connection,
            [NotNull] ILogger logger);

        public abstract Task ExecuteAsync(
            [NotNull] IRelationalConnection connection,
            [NotNull] ILogger logger,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}

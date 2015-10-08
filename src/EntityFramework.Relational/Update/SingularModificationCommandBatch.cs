// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Tracing;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Update
{
    public class SingularModificationCommandBatch : AffectedCountModificationCommandBatch
    {
#pragma warning disable 0618
        public SingularModificationCommandBatch(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerator sqlGenerator,
            [NotNull] IUpdateSqlGenerator updateSqlGenerator,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
            [NotNull] ISensitiveDataLogger logger,
            [NotNull] TelemetrySource telemetrySource)
            : base(
                commandBuilderFactory, 
                sqlGenerator, 
                updateSqlGenerator, 
                valueBufferFactoryFactory, 
                logger,
                telemetrySource)
        {
        }
#pragma warning restore 0618

        protected override bool CanAddCommand(ModificationCommand modificationCommand)
            => ModificationCommands.Count == 0;

        protected override bool IsCommandTextValid()
            => true;
    }
}

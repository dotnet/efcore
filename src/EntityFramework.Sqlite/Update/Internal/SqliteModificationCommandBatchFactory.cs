// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Tracing;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Update.Internal
{
    public class SqliteModificationCommandBatchFactory : IModificationCommandBatchFactory
    {
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IUpdateSqlGenerator _updateSqlGenerator;
        private readonly IRelationalValueBufferFactoryFactory _valueBufferFactoryFactory;
        private readonly ISensitiveDataLogger<SqliteModificationCommandBatchFactory> _logger;
        private readonly TelemetrySource _telemetrySource;

        public SqliteModificationCommandBatchFactory(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerator sqlGenerator,
            [NotNull] IUpdateSqlGenerator updateSqlGenerator,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
            [NotNull] ISensitiveDataLogger<SqliteModificationCommandBatchFactory> logger,
            [NotNull] TelemetrySource telemetrySource)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));
            Check.NotNull(updateSqlGenerator, nameof(updateSqlGenerator));
            Check.NotNull(valueBufferFactoryFactory, nameof(valueBufferFactoryFactory));
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(telemetrySource, nameof(telemetrySource));

            _commandBuilderFactory = commandBuilderFactory;
            _sqlGenerator = sqlGenerator;
            _updateSqlGenerator = updateSqlGenerator;
            _valueBufferFactoryFactory = valueBufferFactoryFactory;
            _logger = logger;
            _telemetrySource = telemetrySource;
        }

        public virtual ModificationCommandBatch Create()
            => new SingularModificationCommandBatch(
                _commandBuilderFactory,
                _sqlGenerator,
                _updateSqlGenerator,
                _valueBufferFactoryFactory,
                _logger,
                _telemetrySource);
    }
}

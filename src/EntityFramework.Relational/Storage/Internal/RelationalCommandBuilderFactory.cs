// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Tracing;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class RelationalCommandBuilderFactory : IRelationalCommandBuilderFactory
    {
        private readonly ISensitiveDataLogger _logger;
        private readonly TelemetrySource _telemetrySource;
        private readonly IRelationalTypeMapper _typeMapper;

        public RelationalCommandBuilderFactory(
            [NotNull] ISensitiveDataLogger<RelationalCommandBuilderFactory> logger,
            [NotNull] TelemetrySource telemetrySource,
            [NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(telemetrySource, nameof(telemetrySource));
            Check.NotNull(typeMapper, nameof(typeMapper));

            _logger = logger;
            _telemetrySource = telemetrySource;
            _typeMapper = typeMapper;
        }

        public virtual IRelationalCommandBuilder Create()
            => new RelationalCommandBuilder(
                _logger,
                _telemetrySource,
                _typeMapper);
    }
}

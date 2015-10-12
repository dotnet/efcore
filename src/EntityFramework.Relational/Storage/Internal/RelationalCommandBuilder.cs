// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class RelationalCommandBuilder : IRelationalCommandBuilder
    {
        private readonly ISensitiveDataLogger _logger;
#pragma warning disable 0618
        private readonly TelemetrySource _telemetrySource;
        private readonly IRelationalTypeMapper _typeMapper;
        private readonly List<RelationalParameter> _parameters = new List<RelationalParameter>();

        public RelationalCommandBuilder(
            [NotNull] ISensitiveDataLogger logger,
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
#pragma warning restore 0618

        public virtual IndentedStringBuilder CommandTextBuilder { get; } = new IndentedStringBuilder();

        public virtual IRelationalCommandBuilder AddParameter(
            [NotNull] string name,
            [CanBeNull] object value,
            [NotNull] Func<IRelationalTypeMapper, RelationalTypeMapping> mapType,
            bool? nullable)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(mapType, nameof(mapType));

            _parameters.Add(
                new RelationalParameter(
                    name,
                    value,
                    mapType(_typeMapper),
                    nullable));

            return this;
        }

        public virtual IRelationalCommand BuildRelationalCommand()
                => new RelationalCommand(
                    _logger,
                    _telemetrySource,
                    CommandTextBuilder.ToString(),
                    _parameters);

        public override string ToString() => CommandTextBuilder.ToString();
    }
}

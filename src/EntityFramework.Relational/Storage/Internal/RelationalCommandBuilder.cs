// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class RelationalCommandBuilder : IRelationalCommandBuilder
    {
        private readonly ISensitiveDataLogger _logger;
        private readonly DiagnosticSource _diagnosticSource;
        private readonly IRelationalTypeMapper _typeMapper;

        private readonly List<IRelationalParameter> _parameters = new List<IRelationalParameter>();

        public RelationalCommandBuilder(
            [NotNull] ISensitiveDataLogger logger,
            [NotNull] DiagnosticSource diagnosticSource,
            [NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(diagnosticSource, nameof(diagnosticSource));
            Check.NotNull(typeMapper, nameof(typeMapper));

            _logger = logger;
            _diagnosticSource = diagnosticSource;
            _typeMapper = typeMapper;
        }

        public virtual IndentedStringBuilder CommandTextBuilder { get; } = new IndentedStringBuilder();

        public virtual IRelationalParameter CreateParameter(
            string name,
            object value,
            Func<IRelationalTypeMapper, RelationalTypeMapping> mapType,
            bool? nullable,
            string invariantName)
            => new RelationalParameter(
                name,
                value,
                mapType(_typeMapper),
                nullable,
                invariantName);

        public virtual void AddParameter(IRelationalParameter relationalParameter)
        {
            Check.NotNull(relationalParameter, nameof(relationalParameter));

            _parameters.Add(relationalParameter);
        }

        public virtual IRelationalCommand Build()
            => new RelationalCommand(
                _logger,
                _diagnosticSource,
                CommandTextBuilder.ToString(),
                _parameters);

        public override string ToString() => CommandTextBuilder.ToString();
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class RelationalCommandBuilderFactory : IRelationalCommandBuilderFactory
    {
        private readonly ISensitiveDataLogger _logger;
        private readonly DiagnosticSource _diagnosticSource;
        private readonly IRelationalTypeMapper _typeMapper;

        public RelationalCommandBuilderFactory(
            [NotNull] ISensitiveDataLogger<RelationalCommandBuilderFactory> logger,
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

        public virtual IRelationalCommandBuilder Create()
            => new RelationalCommandBuilder(
                _logger,
                _diagnosticSource,
                _typeMapper);
    }
}

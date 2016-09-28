// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RelationalCommandBuilderFactory : IRelationalCommandBuilderFactory
    {
        private readonly ISensitiveDataLogger _logger;
        private readonly DiagnosticSource _diagnosticSource;
        private readonly IRelationalTypeMapper _typeMapper;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalCommandBuilderFactory(
            [NotNull] ISensitiveDataLogger<IRelationalCommandBuilderFactory> logger,
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IRelationalCommandBuilder Create() => CreateCore(_logger, _diagnosticSource, _typeMapper);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IRelationalCommandBuilder CreateCore(
                [NotNull] ISensitiveDataLogger sensitiveDataLogger,
                [NotNull] DiagnosticSource diagnosticSource,
                [NotNull] IRelationalTypeMapper relationalTypeMapper)
            => new RelationalCommandBuilder(
                sensitiveDataLogger,
                diagnosticSource,
                relationalTypeMapper);
    }
}

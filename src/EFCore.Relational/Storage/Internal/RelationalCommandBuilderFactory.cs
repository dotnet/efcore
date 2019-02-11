// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     <para>
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class RelationalCommandBuilderFactory : IRelationalCommandBuilderFactory
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _logger;
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalCommandBuilderFactory(
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
            [NotNull] IRelationalTypeMappingSource typeMappingSource)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));

            _logger = logger;
            _typeMappingSource = typeMappingSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IRelationalCommandBuilder Create() => CreateCore(_logger, _typeMappingSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IRelationalCommandBuilder CreateCore(
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
            [NotNull] IRelationalTypeMappingSource relationalTypeMappingSource)
            => new RelationalCommandBuilder(
                logger,
                relationalTypeMappingSource);
    }
}

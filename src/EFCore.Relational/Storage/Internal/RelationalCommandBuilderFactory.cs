// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        private readonly IDiagnosticsLogger<LoggerCategory.Database.Sql> _sqlLogger;
        private readonly IDiagnosticsLogger<LoggerCategory.Database.DataReader> _readerLogger;
        private readonly IRelationalTypeMapper _typeMapper;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalCommandBuilderFactory(
            [NotNull] IDiagnosticsLogger<LoggerCategory.Database.Sql> sqlLogger,
            [NotNull] IDiagnosticsLogger<LoggerCategory.Database.DataReader> readerLogger,
            [NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(sqlLogger, nameof(sqlLogger));
            Check.NotNull(readerLogger, nameof(readerLogger));
            Check.NotNull(typeMapper, nameof(typeMapper));

            _sqlLogger = sqlLogger;
            _readerLogger = readerLogger;
            _typeMapper = typeMapper;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IRelationalCommandBuilder Create() => CreateCore(_sqlLogger, _readerLogger, _typeMapper);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IRelationalCommandBuilder CreateCore(
                [NotNull] IDiagnosticsLogger<LoggerCategory.Database.Sql> sqlLogger,
                [NotNull] IDiagnosticsLogger<LoggerCategory.Database.DataReader> readerLogger,
                [NotNull] IRelationalTypeMapper relationalTypeMapper)
            => new RelationalCommandBuilder(
                sqlLogger,
                readerLogger,
                relationalTypeMapper);
    }
}

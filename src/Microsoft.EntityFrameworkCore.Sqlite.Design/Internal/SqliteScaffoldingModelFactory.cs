// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteScaffoldingModelFactory : RelationalScaffoldingModelFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqliteScaffoldingModelFactory(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IDatabaseModelFactory databaseModelFactory,
            [NotNull] CandidateNamingService candidateNamingService)
            : base(loggerFactory, typeMapper, databaseModelFactory, candidateNamingService)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IModel Create(string connectionString, TableSelectionSet tableSelectionSet)
        {
            if (tableSelectionSet != null
                && tableSelectionSet.Schemas.Any())
            {
                Logger.LogWarning(
                    SqliteDesignEventId.SchemasNotSupportedWarning,
                    () => SqliteDesignStrings.UsingSchemaSelectionsWarning);

                // we've logged a general warning above that sqlite ignores all
                // schema selections so mark all of them as matched so that we don't
                // also log warnings about not matching each individual selection
                tableSelectionSet.Schemas.ToList().ForEach(s => s.IsMatched = true);
            }

            var model = base.Create(connectionString, tableSelectionSet);
            model.Scaffolding().UseProviderMethodName = nameof(SqliteDbContextOptionsBuilderExtensions.UseSqlite);
            return model;
        }
    }
}

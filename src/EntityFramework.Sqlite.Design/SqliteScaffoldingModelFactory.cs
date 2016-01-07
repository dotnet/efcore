// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Scaffolding
{
    public class SqliteScaffoldingModelFactory : RelationalScaffoldingModelFactory
    {
        public SqliteScaffoldingModelFactory(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IDatabaseModelFactory databaseModelFactory)
            : base(loggerFactory, typeMapper, databaseModelFactory)
        {
        }

        public override IModel Create(string connectionString, TableSelectionSet tableSelectionSet)
        {
            if (tableSelectionSet != null
                && tableSelectionSet.Schemas.Any())
            {
                Logger.LogWarning(SqliteDesignStrings.UsingSchemaSelectionsWarning);

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

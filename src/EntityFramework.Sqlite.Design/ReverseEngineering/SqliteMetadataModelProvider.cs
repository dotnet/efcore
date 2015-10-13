// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering
{
    public class SqliteMetadataModelProvider : RelationalMetadataModelProvider
    {
        private readonly IMetadataReader _metadataReader;

        public SqliteMetadataModelProvider(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IMetadataReader metadataReader)
            : base(loggerFactory, typeMapper)
        {
            Check.NotNull(metadataReader, nameof(metadataReader));

            _metadataReader = metadataReader;
        }

        public override IModel GetModel([NotNull] string connectionString, [CanBeNull] TableSelectionSet tableSelectionSet)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));

            var databaseInfo = _metadataReader.GetSchema(connectionString, tableSelectionSet ?? TableSelectionSet.InclusiveAll);

            return GetModel(databaseInfo);
        }
    }
}

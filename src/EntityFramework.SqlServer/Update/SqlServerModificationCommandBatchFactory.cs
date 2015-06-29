// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Update
{
    public class SqlServerModificationCommandBatchFactory : ModificationCommandBatchFactory
    {
        public SqlServerModificationCommandBatchFactory(
            [NotNull] ISqlServerUpdateSqlGenerator sqlGenerator)
            : base(sqlGenerator)
        {
        }

        public override ModificationCommandBatch Create(
            IDbContextOptions options,
            IRelationalMetadataExtensionProvider metadataExtensionProvider)
        {
            Check.NotNull(options, nameof(options));
            Check.NotNull(metadataExtensionProvider, nameof(metadataExtensionProvider));

            var optionsExtension = options.Extensions.OfType<SqlServerOptionsExtension>().FirstOrDefault();

            var maxBatchSize = optionsExtension?.MaxBatchSize;

            return new SqlServerModificationCommandBatch((ISqlServerUpdateSqlGenerator)UpdateSqlGenerator, maxBatchSize);
        }
    }
}

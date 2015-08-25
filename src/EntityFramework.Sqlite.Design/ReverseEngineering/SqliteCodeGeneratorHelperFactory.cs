// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering
{
    public class SqliteCodeGeneratorHelperFactory : CodeGeneratorHelperFactory
    {
        private readonly IRelationalMetadataExtensionProvider _extensionsProvider;

        public SqliteCodeGeneratorHelperFactory(
            [NotNull] ModelUtilities modelUtilities,
            [NotNull] IRelationalMetadataExtensionProvider extensionsProvider)
            : base(modelUtilities)
        {
            Check.NotNull(extensionsProvider, nameof(extensionsProvider));

            _extensionsProvider = extensionsProvider;
        }

        public override DbContextCodeGeneratorHelper DbContextHelper(DbContextGeneratorModel generatorModel)
            => new SqliteDbContextCodeGeneratorHelper(generatorModel, _extensionsProvider, ModelUtilities);
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering
{
    public class SqlServerCodeGeneratorHelperFactory : CodeGeneratorHelperFactory
    {
        private readonly IRelationalMetadataExtensionProvider _extensionsProvider;

        public SqlServerCodeGeneratorHelperFactory(
            [NotNull] ModelUtilities modelUtilities,
            [NotNull] IRelationalMetadataExtensionProvider extensionsProvider)
            : base(modelUtilities)
        {
            Check.NotNull(extensionsProvider, nameof(extensionsProvider));

            _extensionsProvider = extensionsProvider;
        }

        public override DbContextCodeGeneratorHelper DbContextHelper(DbContextGeneratorModel generatorModel)
            => new SqlServerDbContextCodeGeneratorHelper(generatorModel, _extensionsProvider, ModelUtilities);
    }
}

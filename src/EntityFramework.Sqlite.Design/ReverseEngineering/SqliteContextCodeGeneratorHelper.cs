// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;

namespace Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering
{
    public class SqliteContextCodeGeneratorHelper : DbContextCodeGeneratorHelper
    {
        public SqliteContextCodeGeneratorHelper(
            [NotNull] DbContextGeneratorModel model, 
            [NotNull] IRelationalMetadataExtensionProvider extensionsProvider)
            : base(model, extensionsProvider)
        {
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerMetadataExtensionProvider : IRelationalMetadataExtensionProvider
    {
        public virtual IRelationalEntityTypeExtensions Extensions(IEntityType entityType) => entityType.SqlServer();
        public virtual IRelationalForeignKeyExtensions Extensions(IForeignKey foreignKey) => foreignKey.SqlServer();
        public virtual IRelationalIndexExtensions Extensions(IIndex index) => index.SqlServer();
        public virtual IRelationalKeyExtensions Extensions(IKey key) => key.SqlServer();
        public virtual IRelationalPropertyExtensions Extensions(IProperty property) => property.SqlServer();
        public virtual IRelationalModelExtensions Extensions(IModel model) => model.SqlServer();
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;

namespace Microsoft.Data.Entity.Sqlite.Metadata
{
    public class SqliteMetadataExtensionProvider : IRelationalMetadataExtensionProvider
    {
        public virtual IRelationalEntityTypeExtensions Extensions(IEntityType entityType) => entityType.Sqlite();
        public virtual IRelationalForeignKeyExtensions Extensions(IForeignKey foreignKey) => foreignKey.Sqlite();
        public virtual IRelationalIndexExtensions Extensions(IIndex index) => index.Sqlite();
        public virtual IRelationalKeyExtensions Extensions(IKey key) => key.Sqlite();
        public virtual IRelationalModelExtensions Extensions(IModel model) => model.Sqlite();
        public virtual IRelationalPropertyExtensions Extensions(IProperty property) => property.Sqlite();
    }
}

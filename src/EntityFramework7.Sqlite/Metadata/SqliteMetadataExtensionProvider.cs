// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Sqlite.Metadata
{
    public class SqliteMetadataExtensionProvider : IRelationalMetadataExtensionProvider
    {
        public virtual IRelationalEntityTypeAnnotations Extensions(IEntityType entityType) => entityType.Sqlite();
        public virtual IRelationalForeignKeyAnnotations Extensions(IForeignKey foreignKey) => foreignKey.Sqlite();
        public virtual IRelationalIndexAnnotations Extensions(IIndex index) => index.Sqlite();
        public virtual IRelationalKeyAnnotations Extensions(IKey key) => key.Sqlite();
        public virtual IRelationalModelAnnotations Extensions(IModel model) => model.Sqlite();
        public virtual IRelationalPropertyAnnotations Extensions(IProperty property) => property.Sqlite();
    }
}

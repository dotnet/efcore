// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Sqlite.Metadata
{
    public class SqliteMetadataExtensionProvider : IRelationalMetadataExtensionProvider
    {
        public virtual IRelationalEntityTypeAnnotations GetAnnotations(IEntityType entityType) => entityType.Sqlite();
        public virtual IRelationalForeignKeyAnnotations GetAnnotations(IForeignKey foreignKey) => foreignKey.Sqlite();
        public virtual IRelationalIndexAnnotations GetAnnotations(IIndex index) => index.Sqlite();
        public virtual IRelationalKeyAnnotations GetAnnotations(IKey key) => key.Sqlite();
        public virtual IRelationalModelAnnotations GetAnnotations(IModel model) => model.Sqlite();
        public virtual IRelationalPropertyAnnotations GetAnnotations(IProperty property) => property.Sqlite();
    }
}

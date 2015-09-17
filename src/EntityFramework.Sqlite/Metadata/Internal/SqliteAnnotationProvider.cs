// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class SqliteAnnotationProvider : IRelationalMetadataExtensionProvider
    {
        public virtual IRelationalEntityTypeAnnotations For(IEntityType entityType) => entityType.Sqlite();
        public virtual IRelationalForeignKeyAnnotations For(IForeignKey foreignKey) => foreignKey.Sqlite();
        public virtual IRelationalIndexAnnotations For(IIndex index) => index.Sqlite();
        public virtual IRelationalKeyAnnotations For(IKey key) => key.Sqlite();
        public virtual IRelationalModelAnnotations For(IModel model) => model.Sqlite();
        public virtual IRelationalPropertyAnnotations For(IProperty property) => property.Sqlite();
    }
}

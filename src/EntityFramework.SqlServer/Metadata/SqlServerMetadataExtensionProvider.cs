// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerMetadataExtensionProvider : IRelationalMetadataExtensionProvider
    {
        public virtual IRelationalEntityTypeAnnotations GetAnnotations(IEntityType entityType) => entityType.SqlServer();
        public virtual IRelationalForeignKeyAnnotations GetAnnotations(IForeignKey foreignKey) => foreignKey.SqlServer();
        public virtual IRelationalIndexAnnotations GetAnnotations(IIndex index) => index.SqlServer();
        public virtual IRelationalKeyAnnotations GetAnnotations(IKey key) => key.SqlServer();
        public virtual IRelationalPropertyAnnotations GetAnnotations(IProperty property) => property.SqlServer();
        public virtual IRelationalModelAnnotations GetAnnotations(IModel model) => model.SqlServer();
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerMetadataExtensionProvider : IRelationalMetadataExtensionProvider
    {
        public virtual IRelationalEntityTypeAnnotations Extensions(IEntityType entityType) => entityType.SqlServer();
        public virtual IRelationalForeignKeyAnnotations Extensions(IForeignKey foreignKey) => foreignKey.SqlServer();
        public virtual IRelationalIndexAnnotations Extensions(IIndex index) => index.SqlServer();
        public virtual IRelationalKeyAnnotations Extensions(IKey key) => key.SqlServer();
        public virtual IRelationalPropertyAnnotations Extensions(IProperty property) => property.SqlServer();
        public virtual IRelationalModelAnnotations Extensions(IModel model) => model.SqlServer();
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerMetadataExtensionsAccessor : IRelationalMetadataExtensionsAccessor
    {
        public virtual ISharedRelationalEntityTypeExtensions For(IEntityType entityType) => entityType.SqlServer();
        public virtual ISharedRelationalForeignKeyExtensions For(IForeignKey foreignKey) => foreignKey.SqlServer();
        public virtual ISharedRelationalIndexExtensions For(IIndex index) => index.SqlServer();
        public virtual ISharedRelationalKeyExtensions For(IKey key) => key.SqlServer();
        public virtual ISharedRelationalPropertyExtensions For(IProperty property) => property.SqlServer();
        public virtual ISharedRelationalModelExtensions For(IModel model) => model.SqlServer();
    }
}

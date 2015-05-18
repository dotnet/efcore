// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;

namespace Microsoft.Data.Entity.Sqlite.Metadata
{
    public class SqliteMetadataExtensionsAccessor : IRelationalMetadataExtensionsAccessor
    {
        // TODO: Update with #875
        public virtual ISharedRelationalEntityTypeExtensions For(IEntityType entityType) => entityType.Relational();
        public virtual ISharedRelationalForeignKeyExtensions For(IForeignKey foreignKey) => foreignKey.Relational();
        public virtual ISharedRelationalIndexExtensions For(IIndex index) => index.Relational();
        public virtual ISharedRelationalKeyExtensions For(IKey key) => key.Relational();
        public virtual ISharedRelationalModelExtensions For(IModel model) => model.Relational();
        public virtual ISharedRelationalPropertyExtensions For(IProperty property) => property.Relational();
    }
}

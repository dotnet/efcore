// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Metadata
{
    public class SqlServerAnnotationProvider : IRelationalAnnotationProvider
    {
        public virtual IRelationalEntityTypeAnnotations For(IEntityType entityType) => entityType.SqlServer();
        public virtual IRelationalForeignKeyAnnotations For(IForeignKey foreignKey) => foreignKey.SqlServer();
        public virtual IRelationalIndexAnnotations For(IIndex index) => index.SqlServer();
        public virtual IRelationalKeyAnnotations For(IKey key) => key.SqlServer();
        public virtual IRelationalPropertyAnnotations For(IProperty property) => property.SqlServer();
        public virtual IRelationalModelAnnotations For(IModel model) => model.SqlServer();
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerMetadataExtensionProvider : IRelationalMetadataExtensionProvider
    {
        private RelationalNameBuilder _nameBuilder;

        public virtual ISqlServerModelExtensions Extensions([NotNull] IModel model)
        {
            return model.SqlServer();
        }

        public virtual ISqlServerEntityTypeExtensions Extensions([NotNull] IEntityType entityType)
        {
            return entityType.SqlServer();
        }

        public virtual ISqlServerPropertyExtensions Extensions([NotNull] IProperty property)
        {
            return property.SqlServer();
        }

        public virtual ISqlServerKeyExtensions Extensions([NotNull] IKey key)
        {
            return key.SqlServer();
        }

        public virtual ISqlServerForeignKeyExtensions Extensions([NotNull] IForeignKey foreignKey)
        {
            return foreignKey.SqlServer();
        }

        public virtual ISqlServerIndexExtensions Extensions([NotNull] IIndex index)
        {
            return index.SqlServer();
        }

        public virtual RelationalNameBuilder NameBuilder
        {
            get { return _nameBuilder ?? (_nameBuilder = new RelationalNameBuilder(this)); }

            [param: NotNull]
            protected set { _nameBuilder = value; }
        }

        IRelationalModelExtensions IRelationalMetadataExtensionProvider.Extensions(IModel model)
        {
            return Extensions(model);
        }

        IRelationalEntityTypeExtensions IRelationalMetadataExtensionProvider.Extensions(IEntityType entityType)
        {
            return Extensions(entityType);
        }

        IRelationalPropertyExtensions IRelationalMetadataExtensionProvider.Extensions(IProperty property)
        {
            return Extensions(property);
        }

        IRelationalKeyExtensions IRelationalMetadataExtensionProvider.Extensions(IKey key)
        {
            return Extensions(key);
        }

        IRelationalForeignKeyExtensions IRelationalMetadataExtensionProvider.Extensions(IForeignKey foreignKey)
        {
            return Extensions(foreignKey);
        }

        IRelationalIndexExtensions IRelationalMetadataExtensionProvider.Extensions(IIndex index)
        {
            return Extensions(index);
        }
    }
}

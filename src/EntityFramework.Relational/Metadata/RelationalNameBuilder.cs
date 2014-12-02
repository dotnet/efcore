// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class RelationalNameBuilder
    {
        private readonly IRelationalMetadataExtensionProvider _extensionProvider;

        public RelationalNameBuilder([NotNull] IRelationalMetadataExtensionProvider extensionProvider)
        {
            Check.NotNull(extensionProvider, "extensionProvider");

            _extensionProvider = extensionProvider;
        }

        public virtual IRelationalMetadataExtensionProvider ExtensionProvider
        {
            get { return _extensionProvider; }
        }

        public virtual string SequenceName([NotNull] ISequence sequence)
        {
            Check.NotNull(sequence, "sequence");

            return sequence.Name;
        }

        public virtual string SequenceSchema([NotNull] ISequence sequence)
        {
            Check.NotNull(sequence, "sequence");

            return sequence.Schema;
        }

        public virtual SchemaQualifiedName SchemaQualifiedSequenceName([NotNull] ISequence sequence)
        {
            return new SchemaQualifiedName(SequenceName(sequence), SequenceSchema(sequence));
        }

        public virtual string TableName([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return ExtensionProvider.Extensions(entityType).Table;
        }

        public virtual string TableSchema([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return ExtensionProvider.Extensions(entityType).Schema;
        }

        public virtual SchemaQualifiedName SchemaQualifiedTableName([NotNull] IEntityType entityType)
        {
            return new SchemaQualifiedName(TableName(entityType), TableSchema(entityType));
        }

        public virtual string ColumnName([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return ExtensionProvider.Extensions(property).Column;
        }

        public virtual string KeyName([NotNull] IKey key)
        {
            Check.NotNull(key, "key");

            return
                ExtensionProvider.Extensions(key).Name
                ?? (key.EntityType.GetPrimaryKey() == key
                    ? string.Format("PK_{0}",
                        FullName(SchemaQualifiedTableName(key.EntityType)))
                    : string.Format("UC_{0}_{1}",
                        FullName(SchemaQualifiedTableName(key.EntityType)),
                        ColumnNames(key.Properties)));
        }

        public virtual string ForeignKeyName([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return
                ExtensionProvider.Extensions(foreignKey).Name
                ?? string.Format(
                    "FK_{0}_{1}_{2}",
                    FullName(SchemaQualifiedTableName(foreignKey.EntityType)),
                    FullName(SchemaQualifiedTableName(foreignKey.ReferencedEntityType)),
                    ColumnNames(foreignKey.Properties));
        }

        public virtual string IndexName([NotNull] IIndex index)
        {
            Check.NotNull(index, "index");

            return
                ExtensionProvider.Extensions(index).Name
                ?? string.Format(
                    "IX_{0}_{1}",
                    FullName(SchemaQualifiedTableName(index.EntityType)),
                    ColumnNames(index.Properties));
        }

        protected virtual string FullName(SchemaQualifiedName schemaQualifiedName)
        {
            return string.IsNullOrEmpty(schemaQualifiedName.Schema)
                ? schemaQualifiedName.Name
                : schemaQualifiedName.Schema + "." + schemaQualifiedName.Name;
        }

        protected virtual string ColumnNames([NotNull] IEnumerable<IProperty> properties)
        {
            Check.NotNull(properties, "properties");

            return string.Join("_",
                properties.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(p => ExtensionProvider.Extensions(p).Column));
        }
    }
}

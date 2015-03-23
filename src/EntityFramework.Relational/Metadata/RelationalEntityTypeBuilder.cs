// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class RelationalEntityTypeBuilder
    {
        private readonly EntityType _entityType;

        public RelationalEntityTypeBuilder([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            _entityType = entityType;
        }

        public virtual RelationalEntityTypeBuilder Table([CanBeNull] string tableName)
        {
            Check.NullButNotEmpty(tableName, nameof(tableName));

            _entityType.Relational().Table = tableName;

            return this;
        }

        public virtual RelationalEntityTypeBuilder Table([CanBeNull] string tableName, [CanBeNull] string schemaName)
        {
            Check.NullButNotEmpty(tableName, nameof(tableName));
            Check.NullButNotEmpty(schemaName, nameof(schemaName));

            _entityType.Relational().Table = tableName;
            _entityType.Relational().Schema = schemaName;

            return this;
        }
    }
}

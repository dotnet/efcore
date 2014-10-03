// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class RelationalEntityBuilder
    {
        private readonly EntityType _entityType;

        public RelationalEntityBuilder([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            _entityType = entityType;
        }

        public virtual RelationalEntityBuilder Table([CanBeNull] string tableName)
        {
            Check.NullButNotEmpty(tableName, "tableName");

            _entityType.Relational().Table = tableName;

            return this;
        }

        public virtual RelationalEntityBuilder Table([CanBeNull] string tableName, [CanBeNull] string schemaName)
        {
            Check.NullButNotEmpty(tableName, "tableName");
            Check.NullButNotEmpty(schemaName, "schemaName");

            _entityType.Relational().Table = tableName;
            _entityType.Relational().Schema = schemaName;

            return this;
        }
    }
}

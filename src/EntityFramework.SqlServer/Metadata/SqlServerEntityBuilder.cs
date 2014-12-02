// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerEntityBuilder
    {
        private readonly EntityType _entityType;

        public SqlServerEntityBuilder([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            _entityType = entityType;
        }

        public virtual SqlServerEntityBuilder Table([CanBeNull] string tableName)
        {
            Check.NullButNotEmpty(tableName, "tableName");

            _entityType.SqlServer().Table = tableName;

            return this;
        }

        public virtual SqlServerEntityBuilder Table([CanBeNull] string tableName, [CanBeNull] string schemaName)
        {
            Check.NullButNotEmpty(tableName, "tableName");
            Check.NullButNotEmpty(schemaName, "schemaName");

            _entityType.SqlServer().Table = tableName;
            _entityType.SqlServer().Schema = schemaName;

            return this;
        }
    }
}

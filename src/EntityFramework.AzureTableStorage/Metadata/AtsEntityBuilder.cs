// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage.Metadata
{
    public class AtsEntityBuilder
    {
        private readonly EntityType _entityType;

        public AtsEntityBuilder([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            _entityType = entityType;
        }

        protected virtual EntityType EntityType
        {
            get { return _entityType; }
        }

        public virtual AtsEntityBuilder Table([CanBeNull] string tableName)
        {
            Check.NullButNotEmpty(tableName, "tableName");

            _entityType.AzureTableStorage().Table = tableName;

            return this;
        }

        public virtual AtsEntityBuilder PartitionAndRowKey(
            [NotNull] string partitionKeyPropertyName,
            [NotNull] string rowKeyPropertyName)
        {
            Check.NotEmpty(partitionKeyPropertyName, "partitionKeyPropertyName");
            Check.NotEmpty(rowKeyPropertyName, "rowKeyPropertyName");

            var partitionProperty = _entityType.GetProperty(partitionKeyPropertyName);
            var rowProperty = _entityType.GetProperty(rowKeyPropertyName);

            partitionProperty.AzureTableStorage().Column = "PartitionKey";
            rowProperty.AzureTableStorage().Column = "RowKey";

            _entityType.GetOrSetPrimaryKey(new[] { partitionProperty, rowProperty });

            return this;
        }

        public virtual AtsEntityBuilder Timestamp(
            [NotNull] string name,
            bool shadowProperty = false)
        {
            Check.NotEmpty(name, "name");

            var property = _entityType.GetOrAddProperty(name, typeof(DateTimeOffset), shadowProperty);

            if (property.PropertyType != typeof(DateTimeOffset))
            {
                throw new InvalidOperationException(
                    Strings.FormatBadTimestampType(property.Name, _entityType.SimpleName, property.PropertyType.Name));
            }

            property.AzureTableStorage().Column = "Timestamp";

            return this;
        }
    }
}

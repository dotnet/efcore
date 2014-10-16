// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage.Metadata
{
    public class AtsEntityBuilder<TEntity> : AtsEntityBuilder
    {
        public AtsEntityBuilder([NotNull] EntityType entityType)
            : base(entityType)
        {
        }

        public new virtual AtsEntityBuilder<TEntity> Table([CanBeNull] string tableName)
        {
            Check.NullButNotEmpty(tableName, "tableName");

            return (AtsEntityBuilder<TEntity>)base.Table(tableName);
        }

        public virtual AtsEntityBuilder<TEntity> PartitionAndRowKey(
            [NotNull] Expression<Func<TEntity, object>> partitionKeyExpression,
            [NotNull] Expression<Func<TEntity, object>> rowKeyExpression)
        {
            Check.NotNull(partitionKeyExpression, "partitionKeyExpression");
            Check.NotNull(rowKeyExpression, "rowKeyExpression");

            return PartitionAndRowKey(
                EntityType.GetOrAddProperty(partitionKeyExpression.GetPropertyAccess()).Name,
                EntityType.GetOrAddProperty(rowKeyExpression.GetPropertyAccess()).Name);
        }

        public new virtual AtsEntityBuilder<TEntity> PartitionAndRowKey(
            [NotNull] string partitionKeyPropertyName,
            [NotNull] string rowKeyPropertyName)
        {
            Check.NotEmpty(partitionKeyPropertyName, "partitionKeyPropertyName");
            Check.NotEmpty(rowKeyPropertyName, "rowKeyPropertyName");

            return (AtsEntityBuilder<TEntity>)base.PartitionAndRowKey(partitionKeyPropertyName, rowKeyPropertyName);
        }

        public virtual AtsEntityBuilder<TEntity> Timestamp(
            [NotNull] Expression<Func<TEntity, object>> expression)
        {
            Check.NotNull(expression, "expression");

            return Timestamp(EntityType.GetOrAddProperty(expression.GetPropertyAccess()).Name);
        }

        public new virtual AtsEntityBuilder<TEntity> Timestamp(
            [NotNull] string name,
            bool shadowProperty = false)
        {
            Check.NotEmpty(name, "name");

            return (AtsEntityBuilder<TEntity>)base.Timestamp(name, shadowProperty);
        }
    }
}

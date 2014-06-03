// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public static class EntityBuilderExtensions
    {
        public static ModelBuilder.EntityBuilder<T> UseDefaultAzureTableKey<T>([NotNull] this ModelBuilder.EntityBuilder<T> builder)
        {
            Check.NotNull(builder, "builder");

            builder.Key(new string[] { "PartitionKey", "RowKey" });
            return builder;
        }

        public static ModelBuilder.EntityBuilder<T> AzureTableProperties<T>([NotNull] this ModelBuilder.EntityBuilder<T> builder, Action<AzureTablePropertiesBuilder<T>> func)
        {
            func.Invoke(new AzureTablePropertiesBuilder<T>(builder));
            return builder;
        } 
    }

    public class AzureTablePropertiesBuilder<T>
    {
        private readonly ModelBuilder.EntityBuilder<T> _builder;

        public AzureTablePropertiesBuilder(ModelBuilder.EntityBuilder<T> builder)
        {
            _builder = builder;
        }

        public ModelBuilder.EntityBuilder<T> PartitionKey(Expression<Func<T, object>> expr)
        {
            return _builder.Properties(pb => pb.Property(expr).StorageName("PartitionKey"));
        }

        public ModelBuilder.EntityBuilder<T> RowKey(Expression<Func<T, object>> expr)
        {
            return _builder.Properties(pb => pb.Property(expr).StorageName("RowKey"));
        }

        public ModelBuilder.EntityBuilder<T> Timestamp(Expression<Func<T, object>> expr)
        {
            return _builder.Properties(pb => pb.Property(expr).StorageName("Timestamp"));
        }

        public ModelBuilder.EntityBuilder<T> Timestamp([NotNull] string name, bool shadowProperty = false)
        {
            Check.NotNull(name, "name");
            return _builder.Properties(pb => pb.Property<T>(name, shadowProperty).StorageName("Timestamp"));
        }
    }
}

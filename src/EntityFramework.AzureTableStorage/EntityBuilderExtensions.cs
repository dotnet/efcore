// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity.Metadata
{
    public static class EntityBuilderExtensions
    {
        public static ModelBuilder.EntityBuilder<T> UseDefaultAzureTableKey<T>([NotNull] this ModelBuilder.EntityBuilder<T> builder)
        {
            Check.NotNull(builder, "builder");

            builder.Key(new string[] { "PartitionKey", "RowKey" });
            return builder;
        }

        public static ModelBuilder.EntityBuilder<T> AzureTableProperties<T>([NotNull] this ModelBuilder.EntityBuilder<T> builder, [NotNull] Action<AzureTablePropertiesBuilder<T>> func)
        {
            Check.NotNull(builder, "builder");
            func.Invoke(new AzureTablePropertiesBuilder<T>(builder));
            return builder;
        } 
    }

    public class AzureTablePropertiesBuilder<T>
    {
        private readonly ModelBuilder.EntityBuilder<T> _builder;

        public AzureTablePropertiesBuilder([NotNull] ModelBuilder.EntityBuilder<T> builder)
        {
            Check.NotNull(builder, "builder");
            _builder = builder;
        }

        public virtual ModelBuilder.EntityBuilder<T> PartitionKey([NotNull] Expression<Func<T, object>> expr)
        {
            Check.NotNull(expr, "expr");
            return _builder.Properties(pb => pb.Property(expr).StorageName("PartitionKey"));
        }

        public virtual ModelBuilder.EntityBuilder<T> RowKey([NotNull] Expression<Func<T, object>> expr)
        {
            Check.NotNull(expr, "expr");
            return _builder.Properties(pb => pb.Property(expr).StorageName("RowKey"));
        }

        public virtual ModelBuilder.EntityBuilder<T> Timestamp([NotNull] Expression<Func<T, object>> expr)
        {
            Check.NotNull(expr, "expr");
            return _builder.Properties(pb => pb.Property(expr).StorageName("Timestamp"));
        }

        public virtual ModelBuilder.EntityBuilder<T> Timestamp([NotNull] string name, bool shadowProperty = false)
        {
            Check.NotNull(name, "name");
            return _builder.Properties(pb => pb.Property<T>(name, shadowProperty).StorageName("Timestamp"));
        }
    }
}

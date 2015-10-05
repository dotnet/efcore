// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class SqlServerPropertyBuilderExtensions
    {
        public static PropertyBuilder HasSqlServerColumnName(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            propertyBuilder.Metadata.SqlServer().ColumnName = name;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> HasSqlServerColumnName<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string name)
            => (PropertyBuilder<TProperty>)HasSqlServerColumnName((PropertyBuilder)propertyBuilder, name);

        public static PropertyBuilder HasSqlServerColumnType(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string typeName)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(typeName, nameof(typeName));

            propertyBuilder.Metadata.SqlServer().ColumnType = typeName;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> HasSqlServerColumnType<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string typeName)
            => (PropertyBuilder<TProperty>)HasSqlServerColumnType((PropertyBuilder)propertyBuilder, typeName);

        public static PropertyBuilder HasSqlServerDefaultValueSql(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string sql)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(sql, nameof(sql));

            propertyBuilder.ValueGeneratedOnAdd();
            propertyBuilder.Metadata.SqlServer().GeneratedValueSql = sql;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> HasSqlServerDefaultValueSql<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string sql)
            => (PropertyBuilder<TProperty>)HasSqlServerDefaultValueSql((PropertyBuilder)propertyBuilder, sql);

        public static PropertyBuilder HasSqlServerDefaultValue(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] object value)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            propertyBuilder.Metadata.SqlServer().DefaultValue = value;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> HasSqlServerDefaultValue<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] object value)
            => (PropertyBuilder<TProperty>)HasSqlServerDefaultValue((PropertyBuilder)propertyBuilder, value);

        public static PropertyBuilder HasSqlServerComputedColumnSql(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string sql)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(sql, nameof(sql));

            propertyBuilder.ValueGeneratedOnAddOrUpdate();
            propertyBuilder.Metadata.SqlServer().GeneratedValueSql = sql;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> HasSqlServerComputedColumnSql<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string sql)
            => (PropertyBuilder<TProperty>)HasSqlServerComputedColumnSql((PropertyBuilder)propertyBuilder, sql);

        public static PropertyBuilder UseSqlServerSequenceHiLo(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var property = propertyBuilder.Metadata;

            name = name ?? SqlServerAnnotationNames.DefaultHiLoSequenceName;

            var model = property.DeclaringEntityType.Model;

            var sequence =
                model.SqlServer().FindSequence(name, schema) ??
                new Sequence(model, SqlServerAnnotationNames.Prefix, name, schema) { IncrementBy = 10 };

            property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo;
            property.ValueGenerated = ValueGenerated.OnAdd;
            property.SqlServer().HiLoSequenceName = name;
            property.SqlServer().HiLoSequenceSchema = schema;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> UseSqlServerSequenceHiLo<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
            => (PropertyBuilder<TProperty>)UseSqlServerSequenceHiLo((PropertyBuilder)propertyBuilder, name, schema);


        public static PropertyBuilder UseSqlServerIdentityColumn(
            [NotNull] this PropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var property = propertyBuilder.Metadata;

            property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.IdentityColumn;
            property.ValueGenerated = ValueGenerated.OnAdd;
            property.SqlServer().HiLoSequenceName = null;
            property.SqlServer().HiLoSequenceSchema = null;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> UseSqlServerIdentityColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => (PropertyBuilder<TProperty>)UseSqlServerIdentityColumn((PropertyBuilder)propertyBuilder);
    }
}

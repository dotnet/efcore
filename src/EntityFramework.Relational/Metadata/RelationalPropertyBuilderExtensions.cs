// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalPropertyBuilderExtensions
    {
        public static PropertyBuilder Column(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string columnName)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(columnName, nameof(columnName));

            propertyBuilder.Metadata.Relational().Column = columnName;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> Column<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string columnName)
            => (PropertyBuilder<TProperty>)Column((PropertyBuilder)propertyBuilder, columnName);

        public static PropertyBuilder ColumnType(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string columnType)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(columnType, nameof(columnType));

            propertyBuilder.Metadata.Relational().ColumnType = columnType;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> ColumnType<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string columnType)
            => (PropertyBuilder<TProperty>)ColumnType((PropertyBuilder)propertyBuilder, columnType);

        public static PropertyBuilder DefaultExpression(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string expression)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(expression, nameof(expression));

            propertyBuilder.Metadata.Relational().DefaultExpression = expression;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> DefaultExpression<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string expression)
            => (PropertyBuilder<TProperty>)DefaultExpression((PropertyBuilder)propertyBuilder, expression);

        public static PropertyBuilder DefaultValue(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] object value)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            propertyBuilder.Metadata.Relational().DefaultValue = value;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> DefaultValue<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] object value)
            => (PropertyBuilder<TProperty>)DefaultValue((PropertyBuilder)propertyBuilder, value);
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public static class RelationalCommandBuilderExtensions
    {
        public static IRelationalCommandBuilder Append(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] object o)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotNull(o, nameof(o));

            commandBuilder.CommandTextBuilder.Append(o);

            return commandBuilder;
        }

        public static IRelationalCommandBuilder AppendLine([NotNull] this IRelationalCommandBuilder commandBuilder)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));

            commandBuilder.CommandTextBuilder.AppendLine();

            return commandBuilder;
        }

        public static IRelationalCommandBuilder AppendLine(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] object o)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotNull(o, nameof(o));

            commandBuilder.CommandTextBuilder.AppendLine(o);

            return commandBuilder;
        }

        public static IRelationalCommandBuilder AppendLines(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] object o)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotNull(o, nameof(o));

            commandBuilder.CommandTextBuilder.AppendLines(o);

            return commandBuilder;
        }

        public static IRelationalCommandBuilder IncrementIndent([NotNull] this IRelationalCommandBuilder commandBuilder)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));

            commandBuilder.CommandTextBuilder.IncrementIndent();

            return commandBuilder;
        }

        public static IRelationalCommandBuilder DecrementIndent([NotNull] this IRelationalCommandBuilder commandBuilder)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));

            commandBuilder.CommandTextBuilder.DecrementIndent();

            return commandBuilder;
        }

        public static IDisposable Indent([NotNull] this IRelationalCommandBuilder commandBuilder)
            => Check.NotNull(commandBuilder, nameof(commandBuilder)).CommandTextBuilder.Indent();

        public static int GetLength([NotNull] this IRelationalCommandBuilder commandBuilder)
            => Check.NotNull(commandBuilder, nameof(commandBuilder)).CommandTextBuilder.Length;

        public static IRelationalCommandBuilder AddParameter(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] string name,
            [CanBeNull] object value)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotEmpty(name, nameof(name));

            return commandBuilder.AddParameter(
                name,
                value,
                t => t.GetMappingForValue(value),
                value?.GetType().IsNullableType());
        }

        public static IRelationalCommandBuilder AddParameter(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] string name,
            [CanBeNull] object value,
            [NotNull] Type type)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(type, nameof(type));

            bool? isNullable = null;

            if (type.IsNullableType())
            {
                isNullable = true;
                type = type.UnwrapNullableType();
            }

            return commandBuilder.AddParameter(
                name,
                value,
                t => t.GetMapping(type),
                isNullable);
        }

        public static IRelationalCommandBuilder AddParameter(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] string name,
            [CanBeNull] object value,
            [NotNull] IProperty property)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(property, nameof(property));

            return commandBuilder.AddParameter(
                name,
                value,
                t => t.GetMapping(property),
                property.IsNullable);
        }
    }
}

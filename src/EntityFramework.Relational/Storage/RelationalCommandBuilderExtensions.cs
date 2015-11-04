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
            [CanBeNull] object value,
            [CanBeNull] string invariantName)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotEmpty(name, nameof(name));

            commandBuilder.AddParameter(
                name,
                value,
                t => t.GetMappingForValue(value),
                value?.GetType().IsNullableType(),
                invariantName);

            return commandBuilder;
        }

        public static IRelationalCommandBuilder AppendParameter(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] string name,
            [CanBeNull] object value,
            [NotNull] Type type,
            [NotNull] string invariantName)
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

            commandBuilder.AddParameter(
                name,
                value,
                t => t.GetMapping(type),
                isNullable,
                invariantName);

            commandBuilder.CommandTextBuilder.Append(name);

            return commandBuilder;
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

            commandBuilder.AddParameter(
                name,
                value,
                t => t.GetMapping(property),
                property.IsNullable,
                invariantName: null);

            return commandBuilder;
        }

        private static void AddParameter(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] string name,
            [CanBeNull] object value,
            [NotNull] Func<IRelationalTypeMapper, RelationalTypeMapping> mapType,
            bool? nullable,
            [CanBeNull] string invariantName)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(mapType, nameof(mapType));

            commandBuilder.AddParameter(
                commandBuilder.CreateParameter(
                    name, value, mapType, nullable, invariantName));
        }
    }
}

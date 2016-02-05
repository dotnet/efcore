// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public static class RelationalCommandBuilderExtensions
    {
        public static IRelationalCommandBuilder Append(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] object o)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotNull(o, nameof(o));

            commandBuilder.Instance.Append(o);

            return commandBuilder;
        }

        public static IRelationalCommandBuilder AppendLine([NotNull] this IRelationalCommandBuilder commandBuilder)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));

            commandBuilder.Instance.AppendLine();

            return commandBuilder;
        }

        public static IRelationalCommandBuilder AppendLine(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] object o)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotNull(o, nameof(o));

            commandBuilder.Instance.AppendLine(o);

            return commandBuilder;
        }

        public static IRelationalCommandBuilder AppendLines(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] object o)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotNull(o, nameof(o));

            commandBuilder.Instance.AppendLines(o);

            return commandBuilder;
        }

        public static IRelationalCommandBuilder IncrementIndent([NotNull] this IRelationalCommandBuilder commandBuilder)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));

            commandBuilder.Instance.IncrementIndent();

            return commandBuilder;
        }

        public static IRelationalCommandBuilder DecrementIndent([NotNull] this IRelationalCommandBuilder commandBuilder)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));

            commandBuilder.Instance.DecrementIndent();

            return commandBuilder;
        }

        public static IDisposable Indent([NotNull] this IRelationalCommandBuilder commandBuilder)
            => Check.NotNull(commandBuilder, nameof(commandBuilder)).Instance.Indent();

        public static int GetLength([NotNull] this IRelationalCommandBuilder commandBuilder)
            => Check.NotNull(commandBuilder, nameof(commandBuilder)).Instance.Length;

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
            Check.NotEmpty(invariantName, nameof(invariantName));

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

            commandBuilder.Instance.Append(name);

            return commandBuilder;
        }

        public static IRelationalCommandBuilder AddParameter(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] string name,
            [CanBeNull] object value,
            [NotNull] IProperty property,
            [NotNull] string invariantName)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(property, nameof(property));
            Check.NotEmpty(invariantName, nameof(invariantName));

            commandBuilder.AddParameter(
                name,
                value,
                t => t.GetMapping(property),
                property.IsNullable,
                invariantName);

            return commandBuilder;
        }

        private static void AddParameter(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] string name,
            [CanBeNull] object value,
            [NotNull] Func<IRelationalTypeMapper, RelationalTypeMapping> mapType,
            bool? nullable,
            [NotNull] string invariantName)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(mapType, nameof(mapType));
            Check.NotEmpty(invariantName, nameof(invariantName));

            commandBuilder.AddParameter(
                commandBuilder.CreateParameter(
                    invariantName, name, mapType, nullable));
        }
    }
}

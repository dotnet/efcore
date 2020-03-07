// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Extension methods for the <see cref="IRelationalCommandBuilder" /> class.
    /// </summary>
    public static class RelationalCommandBuilderExtensions
    {
        /// <summary>
        ///     Appends an object to the command text on a new line.
        /// </summary>
        /// <param name="commandBuilder"> The command builder. </param>
        /// <param name="value"> The object to be written. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IRelationalCommandBuilder AppendLine(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] object value)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotNull(value, nameof(value));

            commandBuilder.Append(value).AppendLine();

            return commandBuilder;
        }

        /// <summary>
        ///     Appends an object, that contains multiple lines of text, to the command text.
        ///     Each line read from the object is appended on a new line.
        /// </summary>
        /// <param name="commandBuilder"> The command builder. </param>
        /// <param name="value"> The object to be written. </param>
        /// <param name="skipFinalNewline"> If <code>true</code>, then the final newline character is skipped. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IRelationalCommandBuilder AppendLines(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] object value,
            bool skipFinalNewline = false)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotNull(value, nameof(value));

            using (var reader = new StringReader(value.ToString()))
            {
                var first = true;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        commandBuilder.AppendLine();
                    }

                    if (line.Length != 0)
                    {
                        commandBuilder.Append(line);
                    }
                }
            }

            if (!skipFinalNewline)
            {
                commandBuilder.AppendLine();
            }

            return commandBuilder;
        }

        /// <summary>
        ///     Increases the indent of the command text.
        /// </summary>
        /// <param name="commandBuilder"> The command builder. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IDisposable Indent([NotNull] this IRelationalCommandBuilder commandBuilder)
            => new Indenter(Check.NotNull(commandBuilder, nameof(commandBuilder)));

        /// <summary>
        ///     Adds a parameter.
        /// </summary>
        /// <param name="commandBuilder"> The command builder. </param>
        /// <param name="invariantName">
        ///     The key that identifies this parameter. Note that <see cref="IRelationalParameter" /> just represents a
        ///     placeholder for a parameter and not the actual value. This is because the same command can be
        ///     reused multiple times with different parameter values.
        /// </param>
        /// <param name="name">
        ///     The name to be used for the parameter when the command is executed against the database.
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IRelationalCommandBuilder AddParameter(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] string invariantName,
            [NotNull] string name)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotEmpty(name, nameof(name));

            return commandBuilder.AddParameter(
                new DynamicRelationalParameter(
                    Check.NotEmpty(invariantName, nameof(invariantName)),
                    Check.NotEmpty(name, nameof(name)),
                    commandBuilder.TypeMappingSource));
        }

        /// <summary>
        ///     Adds a parameter.
        /// </summary>
        /// <param name="commandBuilder"> The command builder. </param>
        /// <param name="invariantName">
        ///     The key that identifies this parameter. Note that <see cref="IRelationalParameter" /> just represents a
        ///     placeholder for a parameter and not the actual value. This is because the same command can be
        ///     reused multiple times with different parameter values.
        /// </param>
        /// <param name="name">
        ///     The name to be used for the parameter when the command is executed against the database.
        /// </param>
        /// <param name="typeMapping">
        ///     The type mapping for the property that values for this parameter will come from.
        /// </param>
        /// <param name="nullable">
        ///     A value indicating whether the parameter can contain null values.
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IRelationalCommandBuilder AddParameter(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] string invariantName,
            [NotNull] string name,
            [NotNull] RelationalTypeMapping typeMapping,
            bool nullable)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(typeMapping, nameof(typeMapping));

            return commandBuilder.AddParameter(
                new TypeMappedRelationalParameter(
                    invariantName,
                    name,
                    typeMapping,
                    nullable));
        }

        /// <summary>
        ///     Adds a parameter.
        /// </summary>
        /// <param name="commandBuilder"> The command builder. </param>
        /// <param name="invariantName">
        ///     The key that identifies this parameter. Note that <see cref="IRelationalParameter" /> just represents a
        ///     placeholder for a parameter and not the actual value. This is because the same command can be
        ///     reused multiple times with different parameter values.
        /// </param>
        /// <param name="name">
        ///     The name to be used for the parameter when the command is executed against the database.
        /// </param>
        /// <param name="property"> The property that the type for this parameter will come from. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IRelationalCommandBuilder AddParameter(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] string invariantName,
            [NotNull] string name,
            [NotNull] IProperty property)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(property, nameof(property));

            return commandBuilder.AddParameter(
                new TypeMappedRelationalParameter(
                    invariantName,
                    name,
                    property.GetRelationalTypeMapping(),
                    property.IsNullable));
        }

        /// <summary>
        ///     Adds a parameter that is ultimately represented as multiple <see cref="DbParameter" />s in the
        ///     final command.
        /// </summary>
        /// <param name="commandBuilder"> The command builder. </param>
        /// <param name="invariantName">
        ///     The key that identifies this parameter. Note that <see cref="IRelationalParameter" /> just represents a
        ///     placeholder for a parameter and not the actual value. This is because the same command can be
        ///     reused multiple times with different parameter values.
        /// </param>
        /// <param name="subParameters"> The parameters to include in the composite. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IRelationalCommandBuilder AddCompositeParameter(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] string invariantName,
            [NotNull] IReadOnlyList<IRelationalParameter> subParameters)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotNull(subParameters, nameof(subParameters));

            if (subParameters.Count > 0)
            {
                commandBuilder.AddParameter(
                    new CompositeRelationalParameter(
                        invariantName,
                        subParameters));
            }

            return commandBuilder;
        }

        /// <summary>
        ///     Adds a parameter.
        /// </summary>
        /// <param name="commandBuilder"> The command builder. </param>
        /// <param name="invariantName">
        ///     The key that identifies this parameter. Note that <see cref="IRelationalParameter" /> just represents a
        ///     placeholder for a parameter and not the actual value. This is because the same command can be
        ///     reused multiple times with different parameter values.
        /// </param>
        /// <param name="dbParameter"> The DbParameter being added. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IRelationalCommandBuilder AddRawParameter(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] string invariantName,
            [NotNull] DbParameter dbParameter)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotNull(dbParameter, nameof(dbParameter));

            return commandBuilder.AddParameter(
                new RawRelationalParameter(invariantName, dbParameter));
        }

        /// <summary>
        ///     Adds a parameter.
        /// </summary>
        /// <param name="commandBuilder"> The command builder. </param>
        /// <param name="invariantName">
        ///     The key that identifies this parameter. Note that <see cref="IRelationalParameter" /> just represents a
        ///     placeholder for a parameter and not the actual value. This is because the same command can be
        ///     reused multiple times with different parameter values.
        /// </param>
        /// <param name="name">
        ///     The name to be used for the parameter when the command is executed against the database.
        /// </param>
        /// <param name="property">
        ///     The property that values for this parameter will come from.
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IRelationalCommandBuilder AddPropertyParameter(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] string invariantName,
            [NotNull] string name,
            [NotNull] IProperty property)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(property, nameof(property));

            return commandBuilder.AddParameter(
                new TypeMappedPropertyRelationalParameter(
                    invariantName,
                    name,
                    property.GetRelationalTypeMapping(),
                    property));
        }

        private sealed class Indenter : IDisposable
        {
            private readonly IRelationalCommandBuilder _builder;

            public Indenter(IRelationalCommandBuilder builder)
            {
                _builder = builder;

                _builder.IncrementIndent();
            }

            public void Dispose() => _builder.DecrementIndent();
        }
    }
}

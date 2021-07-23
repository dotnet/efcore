// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
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
            this IRelationalCommandBuilder commandBuilder,
            string value)
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
        /// <param name="skipFinalNewline"> If <see langword="true" />, then the final newline character is skipped. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IRelationalCommandBuilder AppendLines(
            this IRelationalCommandBuilder commandBuilder,
            string value,
            bool skipFinalNewline = false)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotNull(value, nameof(value));

            using (var reader = new StringReader(value))
            {
                var first = true;
                string? line;
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
        public static IDisposable Indent(this IRelationalCommandBuilder commandBuilder)
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
            this IRelationalCommandBuilder commandBuilder,
            string invariantName,
            string name)
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
            this IRelationalCommandBuilder commandBuilder,
            string invariantName,
            string name,
            RelationalTypeMapping typeMapping,
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
        [Obsolete("Use overload with relationalTypeMapping")]
        public static IRelationalCommandBuilder AddParameter(
            this IRelationalCommandBuilder commandBuilder,
            string invariantName,
            string name,
            IProperty property)
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
        /// <param name="relationalTypeMapping"> The relational type mapping for this parameter. </param>
        /// <param name="nullable"> A value indicating whether the parameter could contain a null value. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IRelationalCommandBuilder AddParameter(
            this IRelationalCommandBuilder commandBuilder,
            string invariantName,
            string name,
            RelationalTypeMapping relationalTypeMapping,
            bool? nullable)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(relationalTypeMapping, nameof(relationalTypeMapping));

            return commandBuilder.AddParameter(
                new TypeMappedRelationalParameter(
                    invariantName,
                    name,
                    relationalTypeMapping,
                    nullable));
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
            this IRelationalCommandBuilder commandBuilder,
            string invariantName,
            IReadOnlyList<IRelationalParameter> subParameters)
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
            this IRelationalCommandBuilder commandBuilder,
            string invariantName,
            DbParameter dbParameter)
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
        [Obsolete("Use overload with relationalTypeMapping")]
        public static IRelationalCommandBuilder AddPropertyParameter(
            this IRelationalCommandBuilder commandBuilder,
            string invariantName,
            string name,
            IProperty property)
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

            public void Dispose()
                => _builder.DecrementIndent();
        }
    }
}

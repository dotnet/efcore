// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Extension methods for the <see cref="IRelationalCommandBuilder" /> class.
    /// </summary>
    public static class RelationalCommandBuilderExtensions
    {
        /// <summary>
        ///     Appends an object to the command text.
        /// </summary>
        /// <param name="commandBuilder"> The command builder. </param>
        /// <param name="o"> The object to be written. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IRelationalCommandBuilder Append(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] object o)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotNull(o, nameof(o));

            commandBuilder.Instance.Append(o);

            return commandBuilder;
        }

        /// <summary>
        ///     Appends a blank line to the command text.
        /// </summary>
        /// <param name="commandBuilder"> The command builder. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IRelationalCommandBuilder AppendLine([NotNull] this IRelationalCommandBuilder commandBuilder)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));

            commandBuilder.Instance.AppendLine();

            return commandBuilder;
        }

        /// <summary>
        ///     Appends an object to the command text on a new line.
        /// </summary>
        /// <param name="commandBuilder"> The command builder. </param>
        /// <param name="o"> The object to be written. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IRelationalCommandBuilder AppendLine(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] object o)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotNull(o, nameof(o));

            commandBuilder.Instance.AppendLine(o);

            return commandBuilder;
        }

        /// <summary>
        ///     Appends an object, that contains multiple lines of text, to the command text.
        ///     Each line read from the object is appended on a new line.
        /// </summary>
        /// <param name="commandBuilder"> The command builder. </param>
        /// <param name="o"> The object to be written. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IRelationalCommandBuilder AppendLines(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] object o)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotNull(o, nameof(o));

            commandBuilder.Instance.AppendLines(o);

            return commandBuilder;
        }

        /// <summary>
        ///     Increments the indent of subsequent lines.
        /// </summary>
        /// <param name="commandBuilder"> The command builder. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IRelationalCommandBuilder IncrementIndent([NotNull] this IRelationalCommandBuilder commandBuilder)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));

            commandBuilder.Instance.IncrementIndent();

            return commandBuilder;
        }

        /// <summary>
        ///     Decrements the indent of subsequent lines.
        /// </summary>
        /// <param name="commandBuilder"> The command builder. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IRelationalCommandBuilder DecrementIndent([NotNull] this IRelationalCommandBuilder commandBuilder)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));

            commandBuilder.Instance.DecrementIndent();

            return commandBuilder;
        }

        /// <summary>
        ///     Increases the indent of the command text.
        /// </summary>
        /// <param name="commandBuilder"> The command builder. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IDisposable Indent([NotNull] this IRelationalCommandBuilder commandBuilder)
            => Check.NotNull(commandBuilder, nameof(commandBuilder)).Instance.Indent();

        /// <summary>
        ///     Gets the length of the command text.
        /// </summary>
        /// <param name="commandBuilder"> The command builder. </param>
        /// <returns> The length of the command text. </returns>
        public static int GetLength([NotNull] this IRelationalCommandBuilder commandBuilder)
            => Check.NotNull(commandBuilder, nameof(commandBuilder)).Instance.Length;

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

            commandBuilder.ParameterBuilder.AddParameter(invariantName, name);

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

            commandBuilder.ParameterBuilder.AddParameter(invariantName, name, typeMapping, nullable);

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

            commandBuilder.ParameterBuilder.AddParameter(invariantName, name, property);

            return commandBuilder;
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
        /// <param name="buildAction">
        ///     The action to add the multiple parameters that this placeholder represents.
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IRelationalCommandBuilder AddCompositeParameter(
            [NotNull] this IRelationalCommandBuilder commandBuilder,
            [NotNull] string invariantName,
            [NotNull] Action<IRelationalParameterBuilder> buildAction)
        {
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotNull(buildAction, nameof(buildAction));

            commandBuilder.ParameterBuilder.AddCompositeParameter(invariantName, buildAction);

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

            commandBuilder.ParameterBuilder.AddRawParameter(invariantName, dbParameter);

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

            commandBuilder.ParameterBuilder.AddPropertyParameter(invariantName, name, property);

            return commandBuilder;
        }
    }
}

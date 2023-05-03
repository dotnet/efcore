// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     Extension methods for the <see cref="IRelationalCommandBuilder" /> class.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public static class RelationalCommandBuilderExtensions
{
    /// <summary>
    ///     Appends an object to the command text on a new line.
    /// </summary>
    /// <param name="commandBuilder">The command builder.</param>
    /// <param name="value">The object to be written.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static IRelationalCommandBuilder AppendLine(
        this IRelationalCommandBuilder commandBuilder,
        string value)
    {
        commandBuilder.Append(value).AppendLine();

        return commandBuilder;
    }

    /// <summary>
    ///     Appends an object, that contains multiple lines of text, to the command text.
    ///     Each line read from the object is appended on a new line.
    /// </summary>
    /// <param name="commandBuilder">The command builder.</param>
    /// <param name="value">The object to be written.</param>
    /// <param name="skipFinalNewline">If <see langword="true" />, then the final newline character is skipped.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static IRelationalCommandBuilder AppendLines(
        this IRelationalCommandBuilder commandBuilder,
        string value,
        bool skipFinalNewline = false)
    {
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
    /// <param name="commandBuilder">The command builder.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static IDisposable Indent(this IRelationalCommandBuilder commandBuilder)
        => new Indenter(commandBuilder);

    /// <summary>
    ///     Adds a parameter.
    /// </summary>
    /// <param name="commandBuilder">The command builder.</param>
    /// <param name="invariantName">
    ///     The key that identifies this parameter. Note that <see cref="IRelationalParameter" /> just represents a
    ///     placeholder for a parameter and not the actual value. This is because the same command can be
    ///     reused multiple times with different parameter values.
    /// </param>
    /// <param name="name">
    ///     The name to be used for the parameter when the command is executed against the database.
    /// </param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    [Obsolete("Use overload which takes TypeMapping argument.")]
    public static IRelationalCommandBuilder AddParameter(
        this IRelationalCommandBuilder commandBuilder,
        string invariantName,
        string name)
        => throw new InvalidOperationException("Use overload which takes TypeMapping argument.");

    /// <summary>
    ///     Adds a parameter.
    /// </summary>
    /// <param name="commandBuilder">The command builder.</param>
    /// <param name="invariantName">
    ///     The key that identifies this parameter. Note that <see cref="IRelationalParameter" /> just represents a
    ///     placeholder for a parameter and not the actual value. This is because the same command can be
    ///     reused multiple times with different parameter values.
    /// </param>
    /// <param name="name">
    ///     The name to be used for the parameter when the command is executed against the database.
    /// </param>
    /// <param name="relationalTypeMapping">The relational type mapping for this parameter.</param>
    /// <param name="nullable">A value indicating whether the parameter could contain a null value.</param>
    /// <param name="direction">The parameter direction.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static IRelationalCommandBuilder AddParameter(
        this IRelationalCommandBuilder commandBuilder,
        string invariantName,
        string name,
        RelationalTypeMapping relationalTypeMapping,
        bool? nullable,
        ParameterDirection direction = ParameterDirection.Input)
        => commandBuilder.AddParameter(
            new TypeMappedRelationalParameter(
                invariantName,
                name,
                relationalTypeMapping,
                nullable,
                direction));

    /// <summary>
    ///     Adds a parameter that is ultimately represented as multiple <see cref="DbParameter" />s in the
    ///     final command.
    /// </summary>
    /// <param name="commandBuilder">The command builder.</param>
    /// <param name="invariantName">
    ///     The key that identifies this parameter. Note that <see cref="IRelationalParameter" /> just represents a
    ///     placeholder for a parameter and not the actual value. This is because the same command can be
    ///     reused multiple times with different parameter values.
    /// </param>
    /// <param name="subParameters">The parameters to include in the composite.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static IRelationalCommandBuilder AddCompositeParameter(
        this IRelationalCommandBuilder commandBuilder,
        string invariantName,
        IReadOnlyList<IRelationalParameter> subParameters)
    {
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
    /// <param name="commandBuilder">The command builder.</param>
    /// <param name="invariantName">
    ///     The key that identifies this parameter. Note that <see cref="IRelationalParameter" /> just represents a
    ///     placeholder for a parameter and not the actual value. This is because the same command can be
    ///     reused multiple times with different parameter values.
    /// </param>
    /// <param name="dbParameter">The DbParameter being added.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static IRelationalCommandBuilder AddRawParameter(
        this IRelationalCommandBuilder commandBuilder,
        string invariantName,
        DbParameter dbParameter)
        => commandBuilder.AddParameter(
            new RawRelationalParameter(invariantName, dbParameter));

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

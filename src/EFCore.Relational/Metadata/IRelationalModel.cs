// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a relational database.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IRelationalModel : IAnnotatable
{
    /// <summary>
    ///     Gets the full model.
    /// </summary>
    IModel Model { get; }

    /// <summary>
    ///     Returns all the tables mapped in the model.
    /// </summary>
    IEnumerable<ITable> Tables { get; }

    /// <summary>
    ///     Returns all the views mapped in the model.
    /// </summary>
    /// <returns>All the views mapped in the model.</returns>
    IEnumerable<IView> Views { get; }

    /// <summary>
    ///     Returns all the SQL queries mapped in the model.
    /// </summary>
    /// <returns>All the SQL queries mapped in the model.</returns>
    IEnumerable<ISqlQuery> Queries { get; }

    /// <summary>
    ///     Returns all sequences contained in the model.
    /// </summary>
    IEnumerable<ISequence> Sequences
        => Model.GetSequences();

    /// <summary>
    ///     Returns all user-defined functions contained in the model.
    /// </summary>
    IEnumerable<IStoreFunction> Functions { get; }

    /// <summary>
    ///     Returns all stored procedures contained in the model.
    /// </summary>
    IEnumerable<IStoreStoredProcedure> StoredProcedures { get; }

    /// <summary>
    ///     Returns the database collation.
    /// </summary>
    string? Collation
        => Model.GetCollation();

    /// <summary>
    ///     Gets the table with the given name. Returns <see langword="null" /> if no table with the given name is defined.
    /// </summary>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <returns>The table with a given name or <see langword="null" /> if no table with the given name is defined.</returns>
    ITable? FindTable(string name, string? schema);

    /// <summary>
    ///     Gets the default table with the given name. Returns <see langword="null" /> if no table with the given name is defined.
    /// </summary>
    /// <param name="name">The name of the table.</param>
    /// <returns>The default table with a given name or <see langword="null" /> if no table with the given name is defined.</returns>
    TableBase? FindDefaultTable(string name);

    /// <summary>
    ///     Gets the view with the given name. Returns <see langword="null" /> if no view with the given name is defined.
    /// </summary>
    /// <param name="name">The name of the view.</param>
    /// <param name="schema">The schema of the view.</param>
    /// <returns>The view with a given name or <see langword="null" /> if no view with the given name is defined.</returns>
    IView? FindView(string name, string? schema);

    /// <summary>
    ///     Gets the SQL query with the given name. Returns <see langword="null" /> if no SQL query with the given name is defined.
    /// </summary>
    /// <param name="name">The name of the SQL query.</param>
    /// <returns>The SQL query with a given name or <see langword="null" /> if no SQL query with the given name is defined.</returns>
    ISqlQuery? FindQuery(string name);

    /// <summary>
    ///     Finds an <see cref="ISequence" /> with the given name.
    /// </summary>
    /// <param name="name">The sequence name.</param>
    /// <param name="schema">The schema that contains the sequence.</param>
    /// <returns>
    ///     The <see cref="ISequence" /> or <see langword="null" /> if no sequence with the given name in
    ///     the given schema was found.
    /// </returns>
    ISequence? FindSequence(string name, string? schema)
        => Model.FindSequence(name, schema);

    /// <summary>
    ///     Finds a <see cref="IStoreFunction" /> with the given signature.
    /// </summary>
    /// <param name="name">The name of the function.</param>
    /// <param name="schema">The schema of the function.</param>
    /// <param name="parameters">A list of parameter types.</param>
    /// <returns>The <see cref="IStoreFunction" /> or <see langword="null" /> if no function with the given name was found.</returns>
    IStoreFunction? FindFunction(string name, string? schema, IReadOnlyList<string> parameters);

    /// <summary>
    ///     Finds a <see cref="IStoreStoredProcedure" /> with the name.
    /// </summary>
    /// <param name="name">The name of the stored procedure.</param>
    /// <param name="schema">The schema of the stored procedure.</param>
    /// <returns>The <see cref="IStoreStoredProcedure" /> or <see langword="null" /> if no stored procedure with the given name was found.</returns>
    IStoreStoredProcedure? FindStoredProcedure(string name, string? schema);

    /// <summary>
    ///     <para>
    ///         Creates a human-readable representation of the given metadata.
    ///     </para>
    ///     <para>
    ///         Warning: Do not rely on the format of the returned string.
    ///         It is designed for debugging only and may change arbitrarily between releases.
    ///     </para>
    /// </summary>
    /// <param name="options">Options for generating the string.</param>
    /// <param name="indent">The number of indent spaces to use before each new line.</param>
    /// <returns>A human-readable representation.</returns>
    string ToDebugString(MetadataDebugStringOptions options = MetadataDebugStringOptions.ShortDefault, int indent = 0)
    {
        var builder = new StringBuilder();
        var indentString = new string(' ', indent);

        try
        {
            builder.Append(indentString).Append("RelationalModel: ");

            if ((Model is Model) && Collation != null)
            {
                builder.AppendLine().Append(indentString).Append("Collation: ").Append(Collation);
            }

            foreach (var table in Tables)
            {
                builder.AppendLine().Append(table.ToDebugString(options, indent + 2));
            }

            foreach (var view in Views)
            {
                builder.AppendLine().Append(view.ToDebugString(options, indent + 2));
            }

            foreach (var function in Functions)
            {
                builder.AppendLine().Append(function.ToDebugString(options, indent + 2));
            }

            foreach (var query in Queries)
            {
                builder.AppendLine().Append(query.ToDebugString(options, indent + 2));
            }

            foreach (var sequence in Sequences)
            {
                builder.AppendLine().Append(sequence.ToDebugString(options, indent + 2));
            }

            if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(AnnotationsToDebugString(indent));
            }
        }
        catch (Exception exception)
        {
            builder.AppendLine().AppendLine(CoreStrings.DebugViewError(exception.Message));
        }

        return builder.ToString();
    }
}

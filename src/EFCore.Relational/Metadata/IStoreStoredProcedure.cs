// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a stored procedure in a database.
/// </summary>
public interface IStoreStoredProcedure : ITableBase
{
    /// <summary>
    ///     Gets the associated model stored procedures.
    /// </summary>
    IEnumerable<IStoredProcedure> StoredProcedures { get; }

    /// <summary>
    ///     Gets the entity type mappings.
    /// </summary>
    new IEnumerable<IStoredProcedureMapping> EntityTypeMappings { get; }

    /// <summary>
    ///     Gets the return for this stored procedure.
    /// </summary>
    IStoreStoredProcedureReturnValue? ReturnValue { get; }

    /// <summary>
    ///     Gets the parameters for this stored procedure.
    /// </summary>
    IReadOnlyList<IStoreStoredProcedureParameter> Parameters { get; }

    /// <summary>
    ///     Gets the parameter with the given name. Returns <see langword="null" />
    ///     if no parameter with the given name is defined for the returned row set.
    /// </summary>
    IStoreStoredProcedureParameter? FindParameter(string name);

    /// <summary>
    ///     Gets the parameter mapped to the given property. Returns <see langword="null" />
    ///     if no parameter is mapped to the given property.
    /// </summary>
    IStoreStoredProcedureParameter? FindParameter(IProperty property);

    /// <summary>
    ///     Gets the columns defined for the returned row set.
    /// </summary>
    IEnumerable<IStoreStoredProcedureResultColumn> ResultColumns { get; }

    /// <summary>
    ///     Gets the result column with the given name. Returns <see langword="null" />
    ///     if no result column with the given name is defined for the returned row set.
    /// </summary>
    IStoreStoredProcedureResultColumn? FindResultColumn(string name);

    /// <summary>
    ///     Gets the result column mapped to the given property. Returns <see langword="null" />
    ///     if no result column is mapped to the given property.
    /// </summary>
    IStoreStoredProcedureResultColumn? FindResultColumn(IProperty property);

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
    string ITableBase.ToDebugString(MetadataDebugStringOptions options, int indent)
    {
        var builder = new StringBuilder();
        var indentString = new string(' ', indent);

        try
        {
            builder
                .Append(indentString)
                .Append("StoreStoredProcedure: ");

            if (Schema != null)
            {
                builder
                    .Append(Schema)
                    .Append('.');
            }

            builder.Append(Name);

            if ((options & MetadataDebugStringOptions.SingleLine) == 0)
            {
                var parameters = Parameters.ToList();
                if (parameters.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Parameters: ");
                    foreach (var parameter in parameters)
                    {
                        builder.AppendLine().Append(parameter.ToDebugString(options, indent + 4));
                    }
                }

                var resultColumns = ResultColumns.ToList();
                if (resultColumns.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  ResultColumns: ");
                    foreach (var column in resultColumns)
                    {
                        builder.AppendLine().Append(column.ToDebugString(options, indent + 4));
                    }
                }

                var mappings = EntityTypeMappings.ToList();
                if (mappings.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  EntityTypeMappings: ");
                    foreach (var mapping in mappings)
                    {
                        builder.AppendLine().Append(mapping.ToDebugString(options, indent + 4));
                    }
                }

                if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
                {
                    builder.Append(AnnotationsToDebugString(indent: indent + 2));
                }
            }
        }
        catch (Exception exception)
        {
            builder.AppendLine().AppendLine(CoreStrings.DebugViewError(exception.Message));
        }

        return builder.ToString();
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a function in the database.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
/// </remarks>
public interface IStoreFunction : ITableBase
{
    /// <summary>
    ///     Gets the associated model functions.
    /// </summary>
    IEnumerable<IDbFunction> DbFunctions { get; }

    /// <summary>
    ///     Gets the value indicating whether the database function is built-in.
    /// </summary>
    bool IsBuiltIn { get; }

    /// <summary>
    ///     Gets the parameters for this function.
    /// </summary>
    IEnumerable<IStoreFunctionParameter> Parameters { get; }

    /// <summary>
    ///     Gets the scalar return type.
    /// </summary>
    string? ReturnType { get; }

    /// <summary>
    ///     Gets the entity type mappings for the returned row set.
    /// </summary>
    new IEnumerable<IFunctionMapping> EntityTypeMappings { get; }

    /// <summary>
    ///     Gets the columns defined for the returned row set.
    /// </summary>
    new IEnumerable<IFunctionColumn> Columns { get; }

    /// <summary>
    ///     Gets the column with the given name. Returns <see langword="null" />
    ///     if no column with the given name is defined for the returned row set.
    /// </summary>
    new IFunctionColumn? FindColumn(string name);

    /// <summary>
    ///     Gets the column mapped to the given property. Returns <see langword="null" /> if no column is mapped to the given property.
    /// </summary>
    new IFunctionColumn? FindColumn(IProperty property);

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
                .Append("StoreFunction: ");

            if (ReturnType != null)
            {
                builder.Append(ReturnType);
            }
            else
            {
                builder.Append(EntityTypeMappings.FirstOrDefault()?.TypeBase.DisplayName() ?? "");
            }

            builder.Append(' ');

            if (Schema != null)
            {
                builder
                    .Append(Schema)
                    .Append('.');
            }

            builder.Append(Name);

            if (IsBuiltIn)
            {
                builder.Append(" IsBuiltIn");
            }

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

                var mappings = EntityTypeMappings.ToList();
                if (mappings.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  EntityTypeMappings: ");
                    foreach (var mapping in mappings)
                    {
                        builder.AppendLine().Append(mapping.ToDebugString(options, indent + 4));
                    }
                }

                var columns = Columns.ToList();
                if (columns.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Columns: ");
                    foreach (var column in columns)
                    {
                        builder.AppendLine().Append(column.ToDebugString(options, indent + 4));
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

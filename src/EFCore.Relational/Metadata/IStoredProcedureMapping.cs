// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents entity type mapping to a stored procedure.
/// </summary>
public interface IStoredProcedureMapping : ITableMappingBase
{
    /// <summary>
    ///     Gets the target stored procedure in the database.
    /// </summary>
    IStoreStoredProcedure StoreStoredProcedure { get; }

    /// <summary>
    ///     Gets the target stored procedure in the model.
    /// </summary>
    IStoredProcedure StoredProcedure { get; }

    /// <summary>
    ///     Gets the stored procedure identifier including whether it's used for insert, delete or update.
    /// </summary>
    StoreObjectIdentifier StoredProcedureIdentifier { get; }

    /// <summary>
    ///     Gets the corresponding table mapping if it exists.
    /// </summary>
    ITableMapping? TableMapping { get; }

    /// <summary>
    ///     Gets the parameter mappings corresponding to the target stored procedure.
    /// </summary>
    IEnumerable<IStoredProcedureParameterMapping> ParameterMappings { get; }

    /// <summary>
    ///     Gets the result column mappings corresponding to the target stored procedure.
    /// </summary>
    IEnumerable<IStoredProcedureResultColumnMapping> ResultColumnMappings { get; }

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
    string ITableMappingBase.ToDebugString(MetadataDebugStringOptions options, int indent)
    {
        var builder = new StringBuilder();
        var indentString = new string(' ', indent);

        builder.Append(indentString);

        var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
        if (singleLine)
        {
            builder.Append("StoredProcedureMapping: ");
        }

        builder.Append(TypeBase.DisplayName()).Append(" - ");

        builder.Append(StoreStoredProcedure.Name);

        builder.Append(" Type:").Append(StoredProcedureIdentifier.StoreObjectType);

        if (IncludesDerivedTypes != null)
        {
            builder.Append(' ');
            if (!IncludesDerivedTypes.Value)
            {
                builder.Append('!');
            }

            builder.Append("IncludesDerivedTypes");
        }

        if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
        {
            builder.Append(AnnotationsToDebugString(indent + 2));
        }

        return builder.ToString();
    }
}

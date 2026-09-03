// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents entity type mapping to a SQL query.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-raw-sql">Executing raw SQL commands with EF Core</see>
///     for more information and examples.
/// </remarks>
public interface ISqlQueryMapping : ITableMappingBase
{
    /// <summary>
    ///     Gets the value indicating whether this is the SQL query mapping
    ///     that should be used when the entity type is queried.
    /// </summary>
    bool IsDefaultSqlQueryMapping { get; set; }

    /// <summary>
    ///     Gets the target SQL query.
    /// </summary>
    ISqlQuery SqlQuery { get; }

    /// <summary>
    ///     Gets the properties mapped to columns on the target SQL query.
    /// </summary>
    new IEnumerable<ISqlQueryColumnMapping> ColumnMappings { get; }

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
            builder.Append("SqlQueryMapping: ");
        }

        builder.Append(TypeBase.Name).Append(" - ");

        builder.Append(Table.Name);

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

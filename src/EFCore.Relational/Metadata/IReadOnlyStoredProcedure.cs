// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a stored procedure in a model.
/// </summary>
public interface IReadOnlyStoredProcedure : IReadOnlyAnnotatable
{
    /// <summary>
    ///     Gets the name of the stored procedure in the database.
    /// </summary>
    string? Name { get; }

    /// <summary>
    ///     Gets the schema of the stored procedure in the database.
    /// </summary>
    string? Schema { get; }

    /// <summary>
    ///     Gets the entity type in which this stored procedure is defined.
    /// </summary>
    IReadOnlyEntityType EntityType { get; }

    /// <summary>
    ///     Returns a value indicating whether automatic creation of transactions is disabled when executing this stored procedure.
    /// </summary>
    /// <returns>The configured value.</returns>
    bool AreTransactionsSuppressed { get; }

    /// <summary>
    ///     Returns the store identifier of this stored procedure.
    /// </summary>
    /// <returns>The store identifier. <see langword="null" /> if there is no corresponding store object.</returns>
    StoreObjectIdentifier? GetStoreIdentifier()
    {
        var name = Name;
        if (name == null)
        {
            return null;
        }

        if (EntityType.GetInsertStoredProcedure() == this)
        {
            return StoreObjectIdentifier.InsertStoredProcedure(name, Schema);
        }

        if (EntityType.GetDeleteStoredProcedure() == this)
        {
            return StoreObjectIdentifier.DeleteStoredProcedure(name, Schema);
        }
        
        if (EntityType.GetUpdateStoredProcedure() == this)
        {
            return StoreObjectIdentifier.UpdateStoredProcedure(name, Schema);
        }

        return null;
    }

    /// <summary>
    ///     Gets the names of properties mapped to parameters for this stored procedure.
    /// </summary>
    IReadOnlyList<string> Parameters { get; }

    /// <summary>
    ///    Returns a value indicating whether there is a parameter corresponding to the given property.
    /// </summary>
    /// <param name="propertyName">The name of a property.</param>
    /// <returns><see langword="true"/> if a parameter corresponding to the given property is found.</returns>
    bool ContainsParameter(string propertyName);

    /// <summary>
    ///     Gets the names of properties mapped to columns of the result for this stored procedure.
    /// </summary>
    IReadOnlyList<string> ResultColumns { get; }

    /// <summary>
    ///    Returns a value indicating whether there is a column of the result corresponding to the given property.
    /// </summary>
    /// <param name="propertyName">The name of a property.</param>
    /// <returns><see langword="true"/> if a columns of the result corresponding to the given property is found.</returns>
    bool ContainsResultColumn(string propertyName);

    /// <summary>
    ///     Returns the name of the stored procedure prepended by the schema
    ///     or <see langword="null" /> if not mapped.
    /// </summary>
    /// <returns>The name of the stored procedure prepended by the schema.</returns>
    string? GetSchemaQualifiedName()
    {
        var name = Name;
        if (name == null)
        {
            return null;
        }

        var schema = Schema;
        return (string.IsNullOrEmpty(schema) ? "" : schema + ".") + name;
    }

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

        builder
            .Append(indentString)
            .Append("StoredProcedure: ");

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
                    builder.AppendLine().Append(parameter);
                }
            }

            if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(AnnotationsToDebugString(indent: indent + 2));
            }
        }

        return builder.ToString();
    }
}

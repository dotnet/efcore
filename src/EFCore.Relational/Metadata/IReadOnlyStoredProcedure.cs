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
    ///     Gets a value indicating whether this stored procedure returns the number of rows affected.
    /// </summary>
    bool IsRowsAffectedReturned { get; }

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
    ///     Gets the parameters for this stored procedure.
    /// </summary>
    IReadOnlyList<IReadOnlyStoredProcedureParameter> Parameters { get; }

    /// <summary>
    ///     Returns the parameter corresponding to the given property.
    /// </summary>
    /// <param name="propertyName">The name of a property.</param>
    /// <returns>The parameter corresponding to the given property if found; <see langword="true" /> otherwise.</returns>
    IReadOnlyStoredProcedureParameter? FindParameter(string propertyName);

    /// <summary>
    ///     Returns the original value parameter corresponding to the given property.
    /// </summary>
    /// <param name="propertyName">The name of a property.</param>
    /// <returns>
    ///     The original value parameter corresponding to the given property if found; <see langword="true" /> otherwise.
    /// </returns>
    IReadOnlyStoredProcedureParameter? FindOriginalValueParameter(string propertyName);

    /// <summary>
    ///     Returns the rows affected parameter.
    /// </summary>
    /// <returns>
    ///     The rows affected parameter if found; <see langword="true" /> otherwise.
    /// </returns>
    IReadOnlyStoredProcedureParameter? FindRowsAffectedParameter();

    /// <summary>
    ///     Gets the columns of the result for this stored procedure.
    /// </summary>
    IReadOnlyList<IReadOnlyStoredProcedureResultColumn> ResultColumns { get; }

    /// <summary>
    ///     Returns the result column corresponding to the given property.
    /// </summary>
    /// <param name="propertyName">The name of a property.</param>
    /// <returns>The result column corresponding to the given property if found; <see langword="true" /> otherwise.</returns>
    IReadOnlyStoredProcedureResultColumn? FindResultColumn(string propertyName);

    /// <summary>
    ///     Returns the rows affected result column.
    /// </summary>
    /// <returns>The rows affected result column if found; <see langword="true" /> otherwise.</returns>
    IReadOnlyStoredProcedureResultColumn? FindRowsAffectedResultColumn();

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

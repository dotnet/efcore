// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a stored procedure in a model in
///     the form that can be mutated while the model is being built.
/// </summary>
public interface IMutableStoredProcedure : IReadOnlyStoredProcedure, IMutableAnnotatable
{
    /// <summary>
    ///     Gets or sets the name of the stored procedure in the database.
    /// </summary>
    new string? Name { get; [param: NotNull] set; }

    /// <summary>
    ///     Gets or sets the schema of the stored procedure in the database.
    /// </summary>
    new string? Schema { get; set; }

    /// <summary>
    ///     Gets the entity type in which this stored procedure is defined.
    /// </summary>
    new IMutableEntityType EntityType { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether this stored procedure returns the number of rows affected.
    /// </summary>
    new bool IsRowsAffectedReturned { get; set; }

    /// <summary>
    ///     Gets the parameters for this stored procedure.
    /// </summary>
    new IReadOnlyList<IMutableStoredProcedureParameter> Parameters { get; }

    /// <summary>
    ///     Returns the parameter corresponding to the given property.
    /// </summary>
    /// <param name="propertyName">The name of a property.</param>
    /// <returns>The parameter corresponding to the given property if found; <see langword="null" /> otherwise.</returns>
    new IMutableStoredProcedureParameter? FindParameter(string propertyName);

    /// <summary>
    ///     Adds a new parameter mapped to the property with the given name.
    /// </summary>
    /// <param name="propertyName">The name of the corresponding property.</param>
    /// <returns>The added parameter.</returns>
    IMutableStoredProcedureParameter AddParameter(string propertyName);

    /// <summary>
    ///     Returns the original value parameter corresponding to the given property.
    /// </summary>
    /// <param name="propertyName">The name of a property.</param>
    /// <returns>
    ///     The original value parameter corresponding to the given property if found; <see langword="null" /> otherwise.
    /// </returns>
    new IMutableStoredProcedureParameter? FindOriginalValueParameter(string propertyName);

    /// <summary>
    ///     Adds a new parameter that holds the original value of the property with the given name.
    /// </summary>
    /// <param name="propertyName">The name of the corresponding property.</param>
    /// <returns>The added parameter.</returns>
    IMutableStoredProcedureParameter AddOriginalValueParameter(string propertyName);

    /// <summary>
    ///     Returns the rows affected parameter.
    /// </summary>
    /// <returns>
    ///     The rows affected parameter if found; <see langword="null" /> otherwise.
    /// </returns>
    new IMutableStoredProcedureParameter? FindRowsAffectedParameter();

    /// <summary>
    ///     Adds an output parameter that returns the rows affected by this stored procedure.
    /// </summary>
    /// <returns>The added parameter.</returns>
    IMutableStoredProcedureParameter AddRowsAffectedParameter();

    /// <summary>
    ///     Gets the columns of the result for this stored procedure.
    /// </summary>
    new IReadOnlyList<IMutableStoredProcedureResultColumn> ResultColumns { get; }

    /// <summary>
    ///     Returns the result column corresponding to the given property.
    /// </summary>
    /// <param name="propertyName">The name of a property.</param>
    /// <returns>The result column corresponding to the given property if found; <see langword="null" /> otherwise.</returns>
    new IMutableStoredProcedureResultColumn? FindResultColumn(string propertyName);

    /// <summary>
    ///     Adds a new column of the result for this stored procedure mapped to the property with the given name
    /// </summary>
    /// <param name="propertyName">The name of the corresponding property.</param>
    /// <returns>The added column.</returns>
    IMutableStoredProcedureResultColumn AddResultColumn(string propertyName);

    /// <summary>
    ///     Returns the rows affected result column.
    /// </summary>
    /// <returns>The rows affected result column if found; <see langword="null" /> otherwise.</returns>
    new IMutableStoredProcedureResultColumn? FindRowsAffectedResultColumn();

    /// <summary>
    ///     Adds a new column of the result that contains the rows affected by this stored procedure.
    /// </summary>
    /// <returns>The added column.</returns>
    IMutableStoredProcedureResultColumn AddRowsAffectedResultColumn();
}

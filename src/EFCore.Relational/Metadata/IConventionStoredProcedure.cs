// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a stored procedure in a model in
///     the form that can be mutated while the model is being built.
/// </summary>
public interface IConventionStoredProcedure : IReadOnlyStoredProcedure, IConventionAnnotatable
{
    /// <summary>
    ///     Gets the entity type in which this stored procedure is defined.
    /// </summary>
    new IConventionEntityType EntityType { get; }

    /// <summary>
    ///     Gets the builder that can be used to configure this stored procedure.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the stored procedure has been removed from the model.</exception>
    new IConventionStoredProcedureBuilder Builder { get; }

    /// <summary>
    ///     Gets the configuration source for this stored procedure.
    /// </summary>
    /// <returns>The configuration source for this stored procedure.</returns>
    ConfigurationSource GetConfigurationSource();

    /// <summary>
    ///     Sets the name of the stored procedure in the database.
    /// </summary>
    /// <param name="name">The name of the stored procedure in the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    string? SetName(string? name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the configuration source for <see cref="IReadOnlyStoredProcedure.Name" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyStoredProcedure.Name" />.</returns>
    ConfigurationSource? GetNameConfigurationSource();

    /// <summary>
    ///     Sets the schema of the stored procedure in the database.
    /// </summary>
    /// <param name="schema">The schema of the stored procedure in the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    string? SetSchema(string? schema, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the configuration source for <see cref="IReadOnlyStoredProcedure.Schema" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyStoredProcedure.Schema" />.</returns>
    ConfigurationSource? GetSchemaConfigurationSource();

    /// <summary>
    ///     Gets the parameters for this stored procedure.
    /// </summary>
    new IReadOnlyList<IConventionStoredProcedureParameter> Parameters { get; }

    /// <summary>
    ///     Returns the parameter corresponding to the given property.
    /// </summary>
    /// <param name="propertyName">The name of a property.</param>
    /// <returns>The parameter corresponding to the given property if found; <see langword="null" /> otherwise.</returns>
    new IConventionStoredProcedureParameter? FindParameter(string propertyName);

    /// <summary>
    ///     Adds a new parameter mapped to the property with the given name.
    /// </summary>
    /// <param name="propertyName">The name of the corresponding property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The added parameter.</returns>
    IConventionStoredProcedureParameter? AddParameter(string propertyName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the original value parameter corresponding to the given property.
    /// </summary>
    /// <param name="propertyName">The name of a property.</param>
    /// <returns>
    ///     The original value parameter corresponding to the given property if found; <see langword="null" /> otherwise.
    /// </returns>
    new IConventionStoredProcedureParameter? FindOriginalValueParameter(string propertyName);

    /// <summary>
    ///     Adds a new parameter that will hold the original value of the property with the given name.
    /// </summary>
    /// <param name="propertyName">The name of the corresponding property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The added parameter.</returns>
    IConventionStoredProcedureParameter? AddOriginalValueParameter(string propertyName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the rows affected parameter.
    /// </summary>
    /// <returns>
    ///     The rows affected parameter if found; <see langword="null" /> otherwise.
    /// </returns>
    new IConventionStoredProcedureParameter? FindRowsAffectedParameter();

    /// <summary>
    ///     Adds an output parameter that returns the rows affected by this stored procedure.
    /// </summary>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The added parameter.</returns>
    IConventionStoredProcedureParameter? AddRowsAffectedParameter(bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the columns of the result for this stored procedure.
    /// </summary>
    new IReadOnlyList<IConventionStoredProcedureResultColumn> ResultColumns { get; }

    /// <summary>
    ///     Returns the result column corresponding to the given property.
    /// </summary>
    /// <param name="propertyName">The name of a property.</param>
    /// <returns>The result column corresponding to the given property if found; <see langword="null" /> otherwise.</returns>
    new IConventionStoredProcedureResultColumn? FindResultColumn(string propertyName);

    /// <summary>
    ///     Adds a new column of the result for this stored procedure mapped to the property with the given name
    /// </summary>
    /// <param name="propertyName">The name of the corresponding property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The added column.</returns>
    IConventionStoredProcedureResultColumn? AddResultColumn(string propertyName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the rows affected result column.
    /// </summary>
    /// <returns>The rows affected result column if found; <see langword="null" /> otherwise.</returns>
    new IConventionStoredProcedureResultColumn? FindRowsAffectedResultColumn();

    /// <summary>
    ///     Adds a new column of the result that contains the rows affected by this stored procedure.
    /// </summary>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The added column.</returns>
    IConventionStoredProcedureResultColumn? AddRowsAffectedResultColumn(bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures whether this stored procedure returns the number of rows affected.
    /// </summary>
    /// <param name="rowsAffectedReturned">A value indicating whether the number of rows affected is returned.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    bool SetIsRowsAffectedReturned(bool rowsAffectedReturned, bool fromDataAnnotation = false);
}

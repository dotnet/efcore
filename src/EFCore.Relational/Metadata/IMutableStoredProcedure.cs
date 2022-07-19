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
    ///     Returns a value indicating whether automatic creation of transactions is disabled when executing this stored procedure.
    /// </summary>
    /// <returns>The configured value.</returns>
    new bool AreTransactionsSuppressed { get; set; }

    /// <summary>
    ///     Adds a new parameter mapped to the property with the given name.
    /// </summary>
    /// <param name="propertyName">The name of the corresponding property.</param>
    /// <returns><see langword="true"/> if a parameter was added.</returns>
    bool AddParameter(string propertyName);

    /// <summary>
    ///     Adds a new column of the result for this stored procedure mapped to the property with the given name
    /// </summary>
    /// <param name="propertyName">The name of the corresponding property.</param>
    /// <returns><see langword="true"/> if a column was added.</returns>
    bool AddResultColumn(string propertyName);
}

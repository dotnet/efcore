// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a stored procedure in a model.
/// </summary>
public interface IStoredProcedure : IReadOnlyStoredProcedure, IAnnotatable
{
    /// <summary>
    ///     Gets the name of the stored procedure in the database.
    /// </summary>
    new string Name { get; }

    /// <summary>
    ///     Gets the entity type in which this stored procedure is defined.
    /// </summary>
    new IEntityType EntityType { get; }

    /// <summary>
    ///     Gets the associated database stored procedure.
    /// </summary>
    IStoreStoredProcedure StoreStoredProcedure { get; }

    /// <summary>
    ///     Returns the store identifier of this stored procedure.
    /// </summary>
    /// <returns>The store identifier.</returns>
    new StoreObjectIdentifier GetStoreIdentifier()
        => ((IReadOnlyStoredProcedure)this).GetStoreIdentifier()!.Value;

}

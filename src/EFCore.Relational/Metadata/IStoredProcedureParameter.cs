// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a stored procedure parameter.
/// </summary>
public interface IStoredProcedureParameter : IReadOnlyStoredProcedureParameter, IAnnotatable
{
    /// <summary>
    ///     Gets the stored procedure to which this parameter belongs.
    /// </summary>
    new IStoredProcedure StoredProcedure { get; }

    /// <summary>
    ///     Gets the associated database stored procedure parameter.
    /// </summary>
    IStoreStoredProcedureParameter StoreParameter { get; }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Creates ad-hoc mappings of CLR types to entity types after the model has been built.
/// </summary>
public interface IAdHocMapper
{
    /// <summary>
    ///     Gets the ad-hoc entity type mapped for the given CLR type, or creates the mapping and returns it if it does not exist.
    /// </summary>
    /// <param name="clrType">The type for which the entity type will be returned.</param>
    /// <returns>The ad-hoc entity type.</returns>
    RuntimeEntityType GetOrAddEntityType(Type clrType);
}

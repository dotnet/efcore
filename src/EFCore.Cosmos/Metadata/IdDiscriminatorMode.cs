// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Defines the behavior for including discriminator values in the JSON "id" value.
/// </summary>
public enum IdDiscriminatorMode
{
    /// <summary>
    ///     No discriminator value is included in the "id" value.
    /// </summary>
    None,

    /// <summary>
    ///     The discriminator value of the entity type is included in the "id" value. This was the default behavior before EF Core 9.
    /// </summary>
    EntityType,

    /// <summary>
    ///     The discriminator value of the root entity type is included in the "id" value. This allows types with the same
    ///     primary key to be saved in the same container, while still allowing "ReadItem" to be used for lookups of an unknown type.
    /// </summary>
    RootEntityType
}

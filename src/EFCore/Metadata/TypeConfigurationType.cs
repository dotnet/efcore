// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     The kind of structural type a CLR type has been explicitly configured as during model building.
/// </summary>
/// <remarks>
///     This type is typically used by database providers (and other extensions). It is generally
///     not used in application code.
/// </remarks>
public enum TypeConfigurationType
{
    /// <summary>
    ///     The type is configured to be ignored.
    /// </summary>
    Ignored,

    /// <summary>
    ///     The type is configured as an entity type.
    /// </summary>
    EntityType,

    /// <summary>
    ///     The type is configured as a shared-type entity type.
    /// </summary>
    SharedTypeEntityType,

    /// <summary>
    ///     The type is configured as an owned entity type.
    /// </summary>
    OwnedEntityType,

    /// <summary>
    ///     The type is configured as a complex type.
    /// </summary>
    ComplexType,

    /// <summary>
    ///     The type is configured as a property.
    /// </summary>
    Property,

    /// <summary>
    ///     The type is configured as a service property.
    /// </summary>
    ServiceProperty
}

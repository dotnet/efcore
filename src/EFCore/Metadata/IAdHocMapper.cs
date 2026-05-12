// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Creates ad-hoc mappings of CLR types to entity types after the model has been built.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public interface IAdHocMapper
{
    /// <summary>
    ///     Gets the ad-hoc entity type mapped for the given CLR type, or creates the mapping and returns it if it does not exist.
    /// </summary>
    /// <param name="clrType">The type for which the entity type will be returned.</param>
    /// <returns>The ad-hoc entity type.</returns>
    RuntimeEntityType GetOrAddEntityType(Type clrType);
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Characteristics of a given EF service.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public readonly struct ServiceCharacteristics
{
    /// <summary>
    ///     Creates a new <see cref="ServiceCharacteristics" /> struct.
    /// </summary>
    /// <param name="lifetime">The service lifetime.</param>
    /// <param name="multipleRegistrations">
    ///     <see langword="true" /> if multiple registrations of the service is allowed; <see langword="false" />
    ///     otherwise.
    /// </param>
    public ServiceCharacteristics(ServiceLifetime lifetime, bool multipleRegistrations = false)
    {
        Lifetime = lifetime;
        MultipleRegistrations = multipleRegistrations;
    }

    /// <summary>
    ///     The service lifetime.
    /// </summary>
    public ServiceLifetime Lifetime { get; }

    /// <summary>
    ///     <see langword="true" /> if multiple registrations of the service is allowed; <see langword="false" /> otherwise.
    /// </summary>
    public bool MultipleRegistrations { get; }
}

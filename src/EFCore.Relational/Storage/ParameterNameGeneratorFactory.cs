// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Creates instances of the <see cref="ParameterNameGenerator" /> type.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class ParameterNameGeneratorFactory : IParameterNameGeneratorFactory
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ParameterNameGeneratorFactory" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public ParameterNameGeneratorFactory(ParameterNameGeneratorDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual ParameterNameGeneratorDependencies Dependencies { get; }

    /// <summary>
    ///     Creates a new <see cref="ParameterNameGenerator" />.
    /// </summary>
    /// <returns>The newly created generator.</returns>
    public virtual ParameterNameGenerator Create()
        => new();
}

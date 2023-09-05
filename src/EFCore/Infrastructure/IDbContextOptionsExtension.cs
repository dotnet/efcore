// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     <para>
///         Interface for extensions that are stored in <see cref="DbContextOptions.Extensions" />.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public interface IDbContextOptionsExtension
{
    /// <summary>
    ///     Information/metadata about the extension.
    /// </summary>
    DbContextOptionsExtensionInfo Info { get; }

    /// <summary>
    ///     Adds the services required to make the selected options work. This is used when there
    ///     is no external <see cref="IServiceProvider" /> and EF is maintaining its own service
    ///     provider internally. This allows database providers (and other extensions) to register their
    ///     required services when EF is creating an service provider.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    void ApplyServices(IServiceCollection services);

    /// <summary>
    ///     Gives the extension a chance to configure defaults based on other options.
    ///     Most extensions do not have dynamic defaults and so this will be a no-op.
    /// </summary>
    /// <param name="options">The options being validated.</param>
    IDbContextOptionsExtension ApplyDefaults(IDbContextOptions options)
        => this;

    /// <summary>
    ///     Gives the extension a chance to validate that all options in the extension are valid.
    ///     Most extensions do not have invalid combinations and so this will be a no-op.
    ///     If options are invalid, then an exception should be thrown.
    /// </summary>
    /// <param name="options">The options being validated.</param>
    void Validate(IDbContextOptions options);
}

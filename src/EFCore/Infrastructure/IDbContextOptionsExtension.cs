// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Interface for extensions that are stored in <see cref="DbContextOptions.Extensions" />.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IDbContextOptionsExtension
    {
        /// <summary>
        ///     Adds the services required to make the selected options work. This is used when there
        ///     is no external <see cref="IServiceProvider" /> and EF is maintaining its own service
        ///     provider internally. This allows database providers (and other extensions) to register their
        ///     required services when EF is creating an service provider.
        /// </summary>
        /// <param name="services"> The collection to add services to. </param>
        /// <returns> True if a database provider and was registered; false otherwise. </returns>
        bool ApplyServices([NotNull] IServiceCollection services);

        /// <summary>
        ///     Returns a hash code created from any options that would cause a new <see cref="IServiceProvider" />
        ///     to be needed. Most extensions do not have any such options and should return zero.
        /// </summary>
        /// <returns> A hash over options that require a new service provider when changed. </returns>
        long GetServiceProviderHashCode();

        /// <summary>
        ///     Gives the extension a chance to validate that all options in the extension are valid.
        ///     Most extensions do not have invalid combinations and so this will be a no-op.
        ///     If options are invalid, then an exception should be thrown.
        /// </summary>
        /// <param name="options"> The options being validated. </param>
        void Validate([NotNull] IDbContextOptions options);

        /// <summary>
        ///     Creates a message fragment for logging typically containing information about
        ///     any useful non-default options that have been configured.
        /// </summary>
        string LogFragment { get; }
    }
}

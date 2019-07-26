// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         This interface must be implemented by any service that needs to be reset between
    ///         different uses of the same <see cref="DbContext" /> in different pools.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/> and multiple registrations
    ///         are allowed. This means that each <see cref="DbContext"/> instance will use its own
    ///         set of instances of this service.
    ///         The implementations may depend on other services registered with any lifetime.
    ///         The implementations do not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IResettableService
    {
        /// <summary>
        ///     Resets the service so that it can be used from the pool.
        /// </summary>
        void ResetState();

        /// <summary>
        ///     Resets the service so that it can be used from the pool.
        /// </summary>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        Task ResetStateAsync(CancellationToken cancellationToken = default);
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Used by EF internal code and database providers to detect concurrent access to non-thread-safe
    ///         resources.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IConcurrencyDetector
    {
        /// <summary>
        ///     Enters a critical section.
        /// </summary>
        /// <returns> A disposer that will exit the critical section when disposed. </returns>
        ConcurrencyDetectorCriticalSectionDisposer EnterCriticalSection();

        /// <summary>
        ///     Exits the critical section.
        /// </summary>
        void ExitCriticalSection();
    }
}

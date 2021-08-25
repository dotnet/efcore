// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     A <see cref="IDisposable" /> returned by an <see cref="IConcurrencyDetector" />, which will exit the ongoing
    ///     critical section when disposed.
    /// </summary>
    /// <remarks>
    ///     For more information, <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>.
    /// </remarks>
    public readonly struct ConcurrencyDetectorCriticalSectionDisposer : IDisposable
    {
        private readonly IConcurrencyDetector _concurrencyDetector;

        /// <summary>
        ///     Constructs a new <see cref="ConcurrencyDetectorCriticalSectionDisposer" />.
        /// </summary>
        /// <param name="concurrencyDetector">
        ///     The <see cref="IConcurrencyDetector" /> on which the critical section will be exited.
        /// </param>
        public ConcurrencyDetectorCriticalSectionDisposer(IConcurrencyDetector concurrencyDetector)
            => _concurrencyDetector = concurrencyDetector;

        /// <inheritdoc />
        public void Dispose()
            => _concurrencyDetector.ExitCriticalSection();
    }
}

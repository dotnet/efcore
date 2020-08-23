// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     A <see cref="IDisposable" /> returned by an <see cref="IConcurrencyDetector" />, which will exit the ongoing
    ///     critical section when disposed.
    /// </summary>
    public readonly struct ConcurrencyDetectorCriticalSectionDisposer : IDisposable
    {
        private readonly IConcurrencyDetector _concurrencyDetector;

        /// <summary>
        ///     Constructs a new <see cref="ConcurrencyDetectorCriticalSectionDisposer" />.
        /// </summary>
        /// <param name="concurrencyDetector">
        ///     The <see cref="IConcurrencyDetector" /> on which the critical section will be exited.
        /// </param>
        public ConcurrencyDetectorCriticalSectionDisposer([NotNull] IConcurrencyDetector concurrencyDetector)
            => _concurrencyDetector = concurrencyDetector;

        /// <inheritdoc />
        public void Dispose()
            => _concurrencyDetector.ExitCriticalSection();
    }
}

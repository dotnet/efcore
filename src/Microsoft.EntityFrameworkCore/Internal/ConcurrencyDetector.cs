// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class ConcurrencyDetector : IConcurrencyDetector
    {
        private readonly IDisposable _disposer;

        private int _inCriticalSection;

        public ConcurrencyDetector()
        {
            _disposer = new Disposer(this);
        }

        public virtual IDisposable EnterCriticalSection()
        {
            if (Interlocked.CompareExchange(ref _inCriticalSection, 1, 0) == 1)
            {
                throw new InvalidOperationException(CoreStrings.ConcurrentMethodInvocation);
            }

            return _disposer;
        }

        private struct Disposer : IDisposable
        {
            private readonly ConcurrencyDetector _concurrencyDetector;

            public Disposer(ConcurrencyDetector concurrencyDetector)
            {
                _concurrencyDetector = concurrencyDetector;
            }

            public void Dispose() => _concurrencyDetector.ExitCriticalSection();
        }

        private void ExitCriticalSection()
        {
            Debug.Assert(_inCriticalSection == 1, "Expected to be in a critical section");

            _inCriticalSection = 0;
        }
    }
}

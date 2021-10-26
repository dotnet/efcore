// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ConcurrencyDetector : IConcurrencyDetector
    {
        private int _inCriticalSection;
        private static readonly AsyncLocal<bool> _threadHasLock = new();
        private int _refCount;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConcurrencyDetectorCriticalSectionDisposer EnterCriticalSection()
        {
            if (Interlocked.CompareExchange(ref _inCriticalSection, 1, 0) == 1)
            {
                if (!_threadHasLock.Value)
                {
                    throw new InvalidOperationException(CoreStrings.ConcurrentMethodInvocation);
                }
            }
            else
            {
                _threadHasLock.Value = true;
            }

            _refCount++;
            return new ConcurrencyDetectorCriticalSectionDisposer(this);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ExitCriticalSection()
        {
            Check.DebugAssert(_inCriticalSection == 1, "Expected to be in a critical section");

            if (--_refCount == 0)
            {
                _threadHasLock.Value = false;
                _inCriticalSection = 0;
            }
        }
    }
}

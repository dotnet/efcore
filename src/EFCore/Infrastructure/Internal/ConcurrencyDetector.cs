// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ConcurrencyDetector : IConcurrencyDetector
{
    private int _inCriticalSection;
    private static readonly AsyncLocal<int> ThreadAcquiredLocksCount = new();
    private int _currentContextRefCount;

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
            if (ThreadAcquiredLocksCount.Value == 0)
            {
                throw new InvalidOperationException(CoreStrings.ConcurrentMethodInvocation);
            }
        }

        ThreadAcquiredLocksCount.Value++;
        _currentContextRefCount++;
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

        ThreadAcquiredLocksCount.Value--;
        if (--_currentContextRefCount == 0)
        {
            _inCriticalSection = 0;
        }
    }
}

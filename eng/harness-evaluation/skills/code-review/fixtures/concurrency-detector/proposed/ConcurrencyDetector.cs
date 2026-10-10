// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ConcurrencyDetector : IConcurrencyDetector, IResettableService
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
        EnterCriticalSectionCore();

        try
        {
            EnterCurrentContext();
        }
        catch
        {
            if (_currentContextRefCount == 0)
            {
                _inCriticalSection = 0;
            }

            throw;
        }

        return new ConcurrencyDetectorCriticalSectionDisposer(this);
    }

    private void EnterCriticalSectionCore()
    {
        if (Interlocked.CompareExchange(ref _inCriticalSection, 1, 0) == 1
            && ThreadAcquiredLocksCount.Value == 0)
        {
            throw new InvalidOperationException(CoreStrings.ConcurrentMethodInvocation);
        }
    }

    private void EnterCurrentContext()
    {
        ThreadAcquiredLocksCount.Value++;
        _currentContextRefCount++;
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

        if (--_currentContextRefCount == 0)
        {
            _inCriticalSection = 0;
        }

        ThreadAcquiredLocksCount.Value--;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ResetState()
    {
        _currentContextRefCount = 0;
        _inCriticalSection = 0;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task ResetStateAsync(CancellationToken cancellationToken = default)
    {
        ResetState();

        return Task.CompletedTask;
    }
}

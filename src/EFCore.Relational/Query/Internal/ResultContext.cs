// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ResultContext
{
    private ResultContext[]? _nestedResultContexts;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object[]? Values { get; set; }

    internal ResultContext[] GetNestedResultContexts(int count)
    {
        if (_nestedResultContexts is not null)
        {
            Check.DebugAssert(
                _nestedResultContexts.Length == count,
                $"Nested result context count '{_nestedResultContexts.Length}' must match requested count '{count}'.");

            return _nestedResultContexts;
        }

        _nestedResultContexts = new ResultContext[count];
        for (var i = 0; i < _nestedResultContexts.Length; i++)
        {
            _nestedResultContexts[i] = new ResultContext();
        }

        return _nestedResultContexts;
    }
}

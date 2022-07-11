// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class QueryDebugView
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Func<string> _toExpressionString;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Func<string> _toQueryString;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public QueryDebugView(
        Func<string> toExpressionString,
        Func<string> toQueryString)
    {
        _toExpressionString = toExpressionString;
        _toQueryString = toQueryString;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Expression
    {
        get
        {
            try
            {
                return _toExpressionString();
            }
            catch (Exception exception)
            {
                return CoreStrings.DebugViewQueryExpressionError(exception.Message);
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Query
    {
        get
        {
            try
            {
                return _toQueryString();
            }
            catch (Exception exception)
            {
                return CoreStrings.DebugViewQueryStringError(exception.Message);
            }
        }
    }
}

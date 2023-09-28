// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Represents a result from an <see cref="IInterceptor" /> such as an <see cref="ISaveChangesInterceptor" /> to allow
///     suppression of the normal operation being intercepted.
/// </summary>
/// <remarks>
///     <para>
///         A value of this type is passed to all interceptor methods that are called before the operation
///         being intercepted is executed.
///         Typically the interceptor should return the value passed in.
///         However, creating a result with <see cref="SuppressWithResult" /> causes the operation being
///         intercepted to be suppressed; that is, the operation is not executed.
///         The value in the result is then used as a substitute return value for the operation that was suppressed.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-interceptors">EF Core interceptors</see> for more information and examples.
///     </para>
/// </remarks>
/// <typeparam name="TResult">The new result to use.</typeparam>
public readonly struct InterceptionResult<TResult>
{
    private readonly TResult _result;

    /// <summary>
    ///     Creates a new <see cref="InterceptionResult{TResult}" /> instance indicating that
    ///     execution should be suppressed and the given result should be used instead.
    /// </summary>
    /// <param name="result">The result to use.</param>
    public static InterceptionResult<TResult> SuppressWithResult(TResult result)
        => new(result);

    private InterceptionResult(TResult result)
    {
        _result = result;
        HasResult = true;
    }

    /// <summary>
    ///     The result to use.
    /// </summary>
    /// <remarks>
    ///     The property can only be accessed if <see cref="HasResult" /> is true. The concept here
    ///     is the same as <see cref="Nullable{T}.Value" /> and <see cref="Nullable{T}.HasValue" />
    /// </remarks>
    /// <exception cref="InvalidOperationException">when <see cref="Result" /> is <see langword="false" />.</exception>
    public TResult Result
    {
        get
        {
            if (!HasResult)
            {
                throw new InvalidOperationException(CoreStrings.NoInterceptionResult);
            }

            return _result;
        }
    }

    /// <summary>
    ///     If true, then interception is suppressed, and <see cref="Result" /> contains the result to use.
    /// </summary>
    public bool HasResult { get; }
}

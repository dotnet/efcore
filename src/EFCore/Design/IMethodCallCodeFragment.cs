// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     Represents a call to a method.
/// </summary>
public interface IMethodCallCodeFragment
{
    /// <summary>
    ///     Gets the name of the method's declaring type.
    /// </summary>
    /// <value>The declaring type's name.</value>
    string? DeclaringType { get; }

    /// <summary>
    ///     Gets the method's name.
    /// </summary>
    /// <value>The method's name.</value>
    string Method { get; }

    /// <summary>
    ///     Gets the method call's generic type arguments.
    /// </summary>
    /// <value>The type arguments.</value>
    IEnumerable<string> TypeArguments { get; }

    /// <summary>
    ///     Gets the method call's arguments.
    /// </summary>
    /// <value>The method call's arguments.</value>
    IEnumerable<object?> Arguments { get; }

    /// <summary>
    ///     Gets the next method call to chain after this.
    /// </summary>
    /// <value>The next method call.</value>
    IMethodCallCodeFragment? ChainedCall { get; }
}

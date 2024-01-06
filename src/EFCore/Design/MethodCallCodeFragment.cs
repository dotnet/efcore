// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     Represents a call to a method.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public class MethodCallCodeFragment : IMethodCallCodeFragment
{
    private readonly List<object?> _arguments;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MethodCallCodeFragment" /> class.
    /// </summary>
    /// <param name="methodInfo">The method's <see cref="MethodInfo" />.</param>
    /// <param name="arguments">The method call's arguments. Can be <see cref="NestedClosureCodeFragment" />.</param>
    public MethodCallCodeFragment(MethodInfo methodInfo, params object?[] arguments)
    {
        var parameters = methodInfo.GetParameters();
        var parameterLength = parameters.Length;
        if (methodInfo.IsDefined(typeof(ExtensionAttribute)))
        {
            parameterLength--;
        }

        if (arguments.Length > parameterLength
            && !parameters[^1].IsDefined(typeof(ParamArrayAttribute)))
        {
            throw new ArgumentException(
                CoreStrings.IncorrectNumberOfArguments(methodInfo.Name, arguments.Length, parameterLength),
                nameof(arguments));
        }

        MethodInfo = methodInfo;
        Namespace = methodInfo.DeclaringType?.Namespace;
        DeclaringType = methodInfo.DeclaringType?.Name;
        Method = methodInfo.Name;
        _arguments = [..arguments];
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MethodCallCodeFragment" /> class.
    /// </summary>
    /// <param name="method">The method's name.</param>
    /// <param name="arguments">
    ///     The method call's arguments. Can be a fragment like <see cref="NestedClosureCodeFragment" /> or
    ///     <see cref="PropertyAccessorCodeFragment" />.
    /// </param>
    public MethodCallCodeFragment(string method, params object?[] arguments)
    {
        Method = method;
        _arguments = [..arguments];
    }

    private MethodCallCodeFragment(
        MethodInfo methodInfo,
        object?[] arguments,
        MethodCallCodeFragment chainedCall)
        : this(methodInfo, arguments)
    {
        ChainedCall = chainedCall;
    }

    private MethodCallCodeFragment(
        string method,
        object?[] arguments,
        MethodCallCodeFragment chainedCall)
        : this(method, arguments)
    {
        ChainedCall = chainedCall;
    }

    /// <summary>
    ///     Gets the <see cref="MethodInfo" /> for this method call.
    /// </summary>
    /// <value> The <see cref="MethodInfo" />. </value>
    public virtual MethodInfo? MethodInfo { get; }

    /// <summary>
    ///     Gets the namespace of the method's declaring type.
    /// </summary>
    /// <value> The declaring type's name. </value>
    public virtual string? Namespace { get; }

    /// <summary>
    ///     Gets the name of the method's declaring type.
    /// </summary>
    /// <value> The declaring type's name. </value>
    public virtual string? DeclaringType { get; }

    /// <summary>
    ///     Gets the method's name.
    /// </summary>
    /// <value> The method's name. </value>
    public virtual string Method { get; }

    IEnumerable<string> IMethodCallCodeFragment.TypeArguments
        => Enumerable.Empty<string>();

    /// <summary>
    ///     Gets the method call's arguments.
    /// </summary>
    /// <value> The method call's arguments. </value>
    public virtual IReadOnlyList<object?> Arguments
        => _arguments;

    IEnumerable<object?> IMethodCallCodeFragment.Arguments
        => Arguments;

    /// <summary>
    ///     Gets the next method call to chain after this.
    /// </summary>
    /// <value> The next method call. </value>
    public virtual MethodCallCodeFragment? ChainedCall { get; }

    IMethodCallCodeFragment? IMethodCallCodeFragment.ChainedCall
        => ChainedCall;

    /// <summary>
    ///     Creates a method chain from this method to another.
    /// </summary>
    /// <param name="methodInfo">The method's <see cref="MethodInfo" />.</param>
    /// <param name="arguments">The next method call's arguments.</param>
    /// <returns>A new fragment representing the method chain.</returns>
    public virtual MethodCallCodeFragment Chain(MethodInfo methodInfo, params object?[] arguments)
        => Chain(new MethodCallCodeFragment(methodInfo, arguments));

    /// <summary>
    ///     Creates a method chain from this method to another.
    /// </summary>
    /// <param name="method">The next method's name.</param>
    /// <param name="arguments">The next method call's arguments.</param>
    /// <returns>A new fragment representing the method chain.</returns>
    public virtual MethodCallCodeFragment Chain(string method, params object?[] arguments)
        => Chain(new MethodCallCodeFragment(method, arguments));

    /// <summary>
    ///     Creates a method chain from this method to another.
    /// </summary>
    /// <param name="call">The next method.</param>
    /// <returns>A new fragment representing the method chain.</returns>
    public virtual MethodCallCodeFragment Chain(MethodCallCodeFragment call)
        => MethodInfo is not null
            ? new MethodCallCodeFragment(MethodInfo, _arguments.ToArray(), ChainedCall?.Chain(call) ?? call)
            : new MethodCallCodeFragment(Method, _arguments.ToArray(), ChainedCall?.Chain(call) ?? call);
}

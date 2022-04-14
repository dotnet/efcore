// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     Represents a call to a method.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public class MethodCallCodeFragment
{
    private readonly List<object?> _arguments;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MethodCallCodeFragment" /> class.
    /// </summary>
    /// <param name="methodInfo">The method's <see cref="MethodInfo" />.</param>
    /// <param name="arguments">The method call's arguments. Can be <see cref="NestedClosureCodeFragment" />.</param>
    public MethodCallCodeFragment(MethodInfo methodInfo, params object?[] arguments)
    {
        var parameterLength = methodInfo.GetParameters().Length;
        if (methodInfo.IsStatic)
        {
            parameterLength--;
        }

        if (arguments.Length > parameterLength)
        {
            throw new ArgumentException(
                CoreStrings.IncorrectNumberOfArguments(methodInfo.Name, arguments.Length, parameterLength),
                nameof(arguments));
        }

        MethodInfo = methodInfo;
        _arguments = new List<object?>(arguments);
    }

    private MethodCallCodeFragment(
        MethodInfo methodInfo,
        MethodCallCodeFragment chainedCall,
        object?[] arguments)
        : this(methodInfo, arguments)
    {
        ChainedCall = chainedCall;
    }

    /// <summary>
    ///     Gets the <see cref="MethodInfo" /> for this method call.
    /// </summary>
    /// <value> The <see cref="MethodInfo" />. </value>
    public virtual MethodInfo MethodInfo { get; }

    /// <summary>
    ///     Gets the namespace of the method's declaring type.
    /// </summary>
    /// <value> The declaring type's name. </value>
    public virtual string? Namespace
        => MethodInfo.DeclaringType?.Namespace;

    /// <summary>
    ///     Gets the name of the method's declaring type.
    /// </summary>
    /// <value> The declaring type's name. </value>
    public virtual string? DeclaringType
        => MethodInfo.DeclaringType?.Name;

    /// <summary>
    ///     Gets the method's name.
    /// </summary>
    /// <value> The method's name. </value>
    public virtual string Method
        => MethodInfo.Name;

    /// <summary>
    ///     Gets the method call's arguments.
    /// </summary>
    /// <value> The method call's arguments. </value>
    public virtual IReadOnlyList<object?> Arguments
        => _arguments;

    /// <summary>
    ///     Gets the next method call to chain after this.
    /// </summary>
    /// <value> The next method call. </value>
    public virtual MethodCallCodeFragment? ChainedCall { get; }

    /// <summary>
    ///     Creates a method chain from this method to another.
    /// </summary>
    /// <param name="methodInfo">The method's <see cref="MethodInfo" />.</param>
    /// <param name="arguments">The next method call's arguments.</param>
    /// <returns>A new fragment representing the method chain.</returns>
    public virtual MethodCallCodeFragment Chain(MethodInfo methodInfo, params object[] arguments)
        => Chain(new MethodCallCodeFragment(methodInfo, arguments));

    /// <summary>
    ///     Creates a method chain from this method to another.
    /// </summary>
    /// <param name="call">The next method.</param>
    /// <returns>A new fragment representing the method chain.</returns>
    public virtual MethodCallCodeFragment Chain(MethodCallCodeFragment call)
        => new(MethodInfo, ChainedCall?.Chain(call) ?? call, _arguments.ToArray());
}

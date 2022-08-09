// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     Represents a nested closure code fragment.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public class NestedClosureCodeFragment
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NestedClosureCodeFragment" /> class.
    /// </summary>
    /// <param name="parameter">The nested closure parameter's name.</param>
    /// <param name="methodCall">The method call used as the body of the nested closure.</param>
    public NestedClosureCodeFragment(string parameter, MethodCallCodeFragment methodCall)
    {
        Parameter = parameter;
        MethodCalls = new List<MethodCallCodeFragment> { methodCall };
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NestedClosureCodeFragment" /> class.
    /// </summary>
    /// <param name="parameter">The nested closure parameter's name.</param>
    /// <param name="methodCalls">The list of method calls used as the body of the nested closure.</param>
    public NestedClosureCodeFragment(string parameter, IReadOnlyList<MethodCallCodeFragment> methodCalls)
    {
        Parameter = parameter;
        MethodCalls = methodCalls;
    }

    /// <summary>
    ///     Gets the nested closure parameter's name.
    /// </summary>
    /// <value>The parameter name.</value>
    public virtual string Parameter { get; }

    /// <summary>
    ///     Gets the method calls used as the body of the nested closure.
    /// </summary>
    /// <value>The method call.</value>
    public virtual IReadOnlyList<MethodCallCodeFragment> MethodCalls { get; }
}

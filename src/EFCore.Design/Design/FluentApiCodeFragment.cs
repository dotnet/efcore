// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     Represents a fluent API method call.
/// </summary>
public class FluentApiCodeFragment : IMethodCallCodeFragment
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FluentApiCodeFragment" /> class.
    /// </summary>
    /// <param name="method">The method's name.</param>
    public FluentApiCodeFragment(string method)
    {
        Method = method;
    }

    /// <summary>
    ///     Gets the namespace of the method's declaring type.
    /// </summary>
    /// <value> The declaring type's name. </value>
    public virtual string? Namespace { get; set; }

    /// <summary>
    ///     Gets the name of the method's declaring type.
    /// </summary>
    /// <value> The declaring type's name. </value>
    public virtual string? DeclaringType { get; set; }

    /// <summary>
    ///     Gets the method's name.
    /// </summary>
    /// <value> The method's name. </value>
    public virtual string Method { get; set; }

    /// <summary>
    ///     Gets the method call's generic type arguments.
    /// </summary>
    /// <value>The type arguments.</value>
    public virtual IList<string> TypeArguments { get; set; } = new List<string>();

    IEnumerable<string> IMethodCallCodeFragment.TypeArguments
        => TypeArguments;

    /// <summary>
    ///     Gets the method call's arguments.
    /// </summary>
    /// <value>The method call's arguments.</value>
    public virtual IList<object?> Arguments { get; set; } = new List<object?>();

    IEnumerable<object?> IMethodCallCodeFragment.Arguments
        => Arguments;

    /// <summary>
    ///     Gets or sets a value indicating whether this method call has an equivalent data annotation.
    /// </summary>
    /// <value>A value indicating whether this method call has an equivalent data annotation.</value>
    public virtual bool IsHandledByDataAnnotations { get; set; }

    /// <summary>
    ///     Gets the next method call to chain after this.
    /// </summary>
    /// <value>The next method call.</value>
    public virtual FluentApiCodeFragment? ChainedCall { get; set; }

    IMethodCallCodeFragment? IMethodCallCodeFragment.ChainedCall
        => ChainedCall;

    /// <summary>
    ///     Creates a new fluent API method call from an existing method call.
    /// </summary>
    /// <param name="call">The existing method call.</param>
    /// <returns>The new fluent API method call.</returns>
    [return: NotNullIfNotNull("call")]
    public static FluentApiCodeFragment? From(MethodCallCodeFragment? call)
        => call is null
            ? null
            : new FluentApiCodeFragment(call.Method)
            {
                Namespace = call.Namespace,
                DeclaringType = call.DeclaringType,
                Arguments = call.Arguments.ToList(),
                ChainedCall = From(call.ChainedCall)
            };

    /// <summary>
    ///     Creates a method chain from this method to another.
    /// </summary>
    /// <param name="call">The next method.</param>
    /// <returns>A new fragment representing the method chain.</returns>
    public virtual FluentApiCodeFragment Chain(FluentApiCodeFragment call)
    {
        var tail = this;
        while (tail.ChainedCall is not null)
        {
            tail = tail.ChainedCall;
        }

        tail.ChainedCall = call;

        return this;
    }

    /// <summary>
    ///     Gets the using statements required for this method chain.
    /// </summary>
    /// <returns>The usings.</returns>
    public virtual IEnumerable<string> GetRequiredUsings()
    {
        var current = this;
        do
        {
            if (current.Namespace is not null)
            {
                yield return current.Namespace;
            }

            foreach (var argumentNamespace in current.Arguments
                         .Where(a => a is not null and not NestedClosureCodeFragment and not PropertyAccessorCodeFragment)
                         .SelectMany(a => a!.GetType().GetNamespaces()))
            {
                yield return argumentNamespace;
            }

            current = current.ChainedCall;
        }
        while (current is not null);
    }

    /// <summary>
    ///     Creates a new method chain with calls filtered based on a predicate.
    /// </summary>
    /// <param name="predicate">A function to test each method call for a condition.</param>
    /// <returns>A new method chain that only contains calls from the original one that satisfy the condition.</returns>
    public virtual FluentApiCodeFragment? FilterChain(Func<FluentApiCodeFragment, bool> predicate)
    {
        FluentApiCodeFragment? newRoot = null;

        var currentLink = this;
        do
        {
            if (predicate(currentLink))
            {
                var unchained = new FluentApiCodeFragment(currentLink.Method)
                {
                    Namespace = currentLink.Namespace,
                    DeclaringType = currentLink.DeclaringType,
                    TypeArguments = currentLink.TypeArguments,
                    Arguments = currentLink.Arguments,
                    IsHandledByDataAnnotations = currentLink.IsHandledByDataAnnotations
                };
                newRoot = newRoot?.Chain(unchained) ?? unchained;
            }

            currentLink = currentLink.ChainedCall;
        }
        while (currentLink is not null);

        return newRoot;
    }
}

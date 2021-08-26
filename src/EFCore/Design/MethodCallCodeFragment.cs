// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     Represents a call to a method.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see> for more information.
    /// </remarks>
    public class MethodCallCodeFragment
    {
        private readonly List<object?> _arguments;

        /// <summary>
        ///     Only used when <see cref="MethodInfo" /> is null
        /// </summary>
        private readonly string? _method;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MethodCallCodeFragment" /> class.
        /// </summary>
        /// <param name="methodInfo"> The method's <see cref="MethodInfo" />. </param>
        /// <param name="arguments"> The method call's arguments. Can be <see cref="NestedClosureCodeFragment" />. </param>
        public MethodCallCodeFragment(MethodInfo methodInfo, params object?[] arguments)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));
            Check.NotNull(arguments, nameof(arguments));

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

        /// <summary>
        ///     Initializes a new instance of the <see cref="MethodCallCodeFragment" /> class.
        /// </summary>
        /// <param name="method"> The method's name. </param>
        /// <param name="arguments"> The method call's arguments. Can be <see cref="NestedClosureCodeFragment" />. </param>
        [Obsolete("Use the overload accepting a MethodInfo")]
        public MethodCallCodeFragment(string method, params object?[] arguments)
        {
            Check.NotEmpty(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));

            _method = method;
            _arguments = new List<object?>(arguments);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MethodCallCodeFragment" /> class.
        /// </summary>
        /// <param name="methodInfo"> The method's <see cref="MethodInfo" />. </param>
        /// <param name="arguments"> The method call's arguments.  Can be <see cref="NestedClosureCodeFragment" />. </param>
        /// <param name="chainedCall"> The next method call to chain after this. </param>
        public MethodCallCodeFragment(
            MethodInfo methodInfo,
            object?[] arguments,
            MethodCallCodeFragment chainedCall)
            : this(methodInfo, arguments)
        {
            Check.NotNull(chainedCall, nameof(chainedCall));

            ChainedCall = chainedCall;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MethodCallCodeFragment" /> class.
        /// </summary>
        /// <param name="method"> The method's name. </param>
        /// <param name="arguments"> The method call's arguments.  Can be <see cref="NestedClosureCodeFragment" />. </param>
        /// <param name="chainedCall"> The next method call to chain after this. </param>
        [Obsolete("Use the overload accepting a MethodInfo")]
        public MethodCallCodeFragment(
            string method,
            object?[] arguments,
            MethodCallCodeFragment chainedCall)
            : this(method, arguments)
        {
            Check.NotNull(chainedCall, nameof(chainedCall));

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
        public virtual string? Namespace
            => MethodInfo?.DeclaringType?.Namespace;

        /// <summary>
        ///     Gets the name of the method's declaring type.
        /// </summary>
        /// <value> The declaring type's name. </value>
        public virtual string? DeclaringType
            => MethodInfo?.DeclaringType?.Name;

        /// <summary>
        ///     Gets the method's name.
        /// </summary>
        /// <value> The method's name. </value>
        public virtual string Method
            => MethodInfo is null ? _method! : MethodInfo.Name;

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
        /// <param name="methodInfo"> The method's <see cref="MethodInfo" />. </param>
        /// <param name="arguments"> The next method call's arguments. </param>
        /// <returns> A new fragment representing the method chain. </returns>
        public virtual MethodCallCodeFragment Chain(MethodInfo methodInfo, params object[] arguments)
            => Chain(new MethodCallCodeFragment(methodInfo, arguments));

        /// <summary>
        ///     Creates a method chain from this method to another.
        /// </summary>
        /// <param name="method"> The next method's name. </param>
        /// <param name="arguments"> The next method call's arguments. </param>
        /// <returns> A new fragment representing the method chain. </returns>
        [Obsolete("Use the overload accepting a MethodInfo")]
        public virtual MethodCallCodeFragment Chain(string method, params object[] arguments)
            => Chain(new MethodCallCodeFragment(method, arguments));

        /// <summary>
        ///     Creates a method chain from this method to another.
        /// </summary>
        /// <param name="call"> The next method. </param>
        /// <returns> A new fragment representing the method chain. </returns>
        public virtual MethodCallCodeFragment Chain(MethodCallCodeFragment call)
            => MethodInfo is null
#pragma warning disable 618
                ? new(_method!, _arguments.ToArray(), ChainedCall?.Chain(call) ?? call)
#pragma warning restore 618
                : new(MethodInfo, _arguments.ToArray(), ChainedCall?.Chain(call) ?? call);
    }
}

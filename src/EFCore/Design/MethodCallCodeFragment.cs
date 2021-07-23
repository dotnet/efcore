// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     Represents a call to a method.
    /// </summary>
    public class MethodCallCodeFragment
    {
        private readonly List<object?> _arguments;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MethodCallCodeFragment" /> class.
        /// </summary>
        /// <param name="method"> The method's name. </param>
        /// <param name="arguments"> The method call's arguments. Can be <see cref="NestedClosureCodeFragment" />. </param>
        public MethodCallCodeFragment(string method, params object?[] arguments)
        {
            Check.NotEmpty(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));

            Method = method;
            _arguments = new List<object?>(arguments);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MethodCallCodeFragment" /> class.
        /// </summary>
        /// <param name="method"> The method's name. </param>
        /// <param name="arguments"> The method call's arguments.  Can be <see cref="NestedClosureCodeFragment" />. </param>
        /// <param name="chainedCall"> The next method call to chain after this. </param>
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
        ///     Gets or sets the method's name.
        /// </summary>
        /// <value> The method's name. </value>
        public virtual string Method { get; }

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
        /// <param name="method"> The next method's name. </param>
        /// <param name="arguments"> The next method call's arguments. </param>
        /// <returns> A new fragment representing the method chain. </returns>
        public virtual MethodCallCodeFragment Chain(string method, params object[] arguments)
            => Chain(new MethodCallCodeFragment(method, arguments));

        /// <summary>
        ///     Creates a method chain from this method to another.
        /// </summary>
        /// <param name="call"> The next method. </param>
        /// <returns> A new fragment representing the method chain. </returns>
        public virtual MethodCallCodeFragment Chain(MethodCallCodeFragment call)
            => new(Method, _arguments.ToArray(), ChainedCall?.Chain(call) ?? call);
    }
}

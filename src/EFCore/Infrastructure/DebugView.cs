// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     A placeholder for lazily-generated debug strings that can be expanded in the debugger to
    ///     to generate and display them.
    /// </summary>
    public class DebugView
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Func<string> _toShortDebugString;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Func<string> _toLongDebugString;

        /// <summary>
        ///     Constructs a new <see cref="DebugView" /> with delegates for short and long debug strings.
        /// </summary>
        /// <param name="toShortDebugString"> Delegate to create the short debug string. </param>
        /// <param name="toLongDebugString"> Delegate to create the long debug string. </param>
        public DebugView(
            Func<string> toShortDebugString,
            Func<string> toLongDebugString)
        {
            _toShortDebugString = toShortDebugString;
            _toLongDebugString = toLongDebugString;
        }

        /// <summary>
        ///     The long-form, detailed debug string.
        /// </summary>
        public virtual string LongView
            => _toLongDebugString();

        /// <summary>
        ///     The short-form, less-detailed debug string.
        /// </summary>
        public virtual string ShortView
            => _toShortDebugString();
    }
}

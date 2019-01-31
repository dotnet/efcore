// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     Used with <see cref="OperationExecutor" /> to handle operation results.
    /// </summary>
    public interface IOperationResultHandler
    {
        /// <summary>
        ///     Gets the contract version of this handler.
        /// </summary>
        /// <value> The contract version of this handler. </value>
        int Version { get; }

        /// <summary>
        ///     Invoked when a result is available.
        /// </summary>
        /// <param name="value"> The result. </param>
        void OnResult([CanBeNull] object value);

        /// <summary>
        ///     Invoked when an error occurs.
        /// </summary>
        /// <param name="type"> The exception type. </param>
        /// <param name="message"> The error message. </param>
        /// <param name="stackTrace"> The stack trace. </param>
        /// <remarks>
        ///     When an <see cref="OperationException" /> is received, the stack trace should not be shown by default.
        /// </remarks>
        void OnError([NotNull] string type, [NotNull] string message, [NotNull] string stackTrace);
    }
}

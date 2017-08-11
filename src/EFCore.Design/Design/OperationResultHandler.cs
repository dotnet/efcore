// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     Used with <see cref="OperationExecutor" /> to handle operation results.
    /// </summary>
    public class OperationResultHandler : MarshalByRefObject, IOperationResultHandler
    {
        private bool _hasResult;
        private object _result;
        private string _errorType;
        private string _errorMessage;
        private string _errorStackTrace;

        /// <summary>
        ///     Gets the contract version of this handler.
        /// </summary>
        /// <value> The contract version of this handler. </value>
        public virtual int Version => 0;

        /// <summary>
        ///     Gets a value indicating whether a result is available.
        /// </summary>
        /// <value>A value indicating whether a result is available.</value>
        public virtual bool HasResult => _hasResult;

        /// <summary>
        ///     Gets the result.
        /// </summary>
        /// <value>The result.</value>
        public virtual object Result => _result;

        /// <summary>
        ///     Gets the type of the exception if any.
        /// </summary>
        /// <value>The exception type.</value>
        public virtual string ErrorType => _errorType;

        /// <summary>
        ///     Gets the error message if any.
        /// </summary>
        /// <value>The error message.</value>
        public virtual string ErrorMessage => _errorMessage;

        /// <summary>
        ///     Get the error stack trace if any.
        /// </summary>
        /// <value> The stack trace. </value>
        /// <remarks>
        ///     When an <see cref="OperationException" /> is recieved, the stack trace should not be shown by default.
        /// </remarks>
        public virtual string ErrorStackTrace => _errorStackTrace;

        /// <summary>
        ///     Invoked when a result is availalbe.
        /// </summary>
        /// <param name="value"> The result. </param>
        public virtual void OnResult(object value)
        {
            _hasResult = true;
            _result = value;
        }

        /// <summary>
        ///     Invoked when an error occurs.
        /// </summary>
        /// <param name="type"> The exception type. </param>
        /// <param name="message"> The error message. </param>
        /// <param name="stackTrace"> The stack trace. </param>
        public virtual void OnError(string type, string message, string stackTrace)
        {
            _errorType = type;
            _errorMessage = message;
            _errorStackTrace = stackTrace;
        }
    }
}

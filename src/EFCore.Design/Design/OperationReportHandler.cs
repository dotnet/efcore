// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     Used to handle reported design-time activity.
    /// </summary>
    public class OperationReportHandler : MarshalByRefObject, IOperationReportHandler
    {
        private readonly Action<string> _errorHandler;
        private readonly Action<string> _warningHandler;
        private readonly Action<string> _informationHandler;
        private readonly Action<string> _verboseHandler;

        /// <summary>
        ///     Gets the contract version of this handler.
        /// </summary>
        /// <value> The contract version of this handler. </value>
        public virtual int Version => 0;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperationReportHandler" /> class.
        /// </summary>
        /// <param name="errorHandler"> A callback for <see cref="OnError(string)" />. </param>
        /// <param name="warningHandler"> A callback for <see cref="OnWarning(string)" />. </param>
        /// <param name="informationHandler"> A callback for <see cref="OnInformation(string)" />. </param>
        /// <param name="verboseHandler"> A callback for <see cref="OnVerbose(string)" />. </param>
        public OperationReportHandler(
            [CanBeNull] Action<string> errorHandler = null,
            [CanBeNull] Action<string> warningHandler = null,
            [CanBeNull] Action<string> informationHandler = null,
            [CanBeNull] Action<string> verboseHandler = null)
        {
            _errorHandler = errorHandler;
            _warningHandler = warningHandler;
            _informationHandler = informationHandler;
            _verboseHandler = verboseHandler;
        }

        /// <summary>
        ///     Invoked when an error is reported.
        /// </summary>
        /// <param name="message"> The message. </param>
        public virtual void OnError(string message)
            => _errorHandler?.Invoke(message);

        /// <summary>
        ///     Invoked when a warning is reported.
        /// </summary>
        /// <param name="message"> The message. </param>
        public virtual void OnWarning(string message)
            => _warningHandler?.Invoke(message);

        /// <summary>
        ///     Invoked when information is reported.
        /// </summary>
        /// <param name="message"> The message. </param>
        public virtual void OnInformation(string message)
            => _informationHandler?.Invoke(message);

        /// <summary>
        ///     Invoked when verbose information is reported.
        /// </summary>
        /// <param name="message"> The message. </param>
        public virtual void OnVerbose(string message)
            => _verboseHandler?.Invoke(message);
    }
}

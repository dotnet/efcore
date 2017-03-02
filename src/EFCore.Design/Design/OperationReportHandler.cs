// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Design
{
    public partial class OperationReportHandler : IOperationReportHandler
    {
        private readonly Action<string> _errorHandler;
        private readonly Action<string> _warningHandler;
        private readonly Action<string> _informationHandler;
        private readonly Action<string> _verboseHandler;

        public virtual int Version => 0;

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

        public virtual void OnError(string message)
            => _errorHandler?.Invoke(message);

        public virtual void OnWarning(string message)
            => _warningHandler?.Invoke(message);

        public virtual void OnInformation(string message)
            => _informationHandler?.Invoke(message);

        public virtual void OnVerbose(string message)
            => _verboseHandler?.Invoke(message);
    }

#if NET451
    public partial class OperationReportHandler : MarshalByRefObject
    {
    }
#endif
}

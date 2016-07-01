// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Design;

namespace Microsoft.EntityFrameworkCore.Tools.Internal
{
    public class OperationLogHandler : DesignMarshalByRefObject, IOperationLogHandler
    {
        private readonly Action<string> _writeError;
        private readonly Action<string> _writeWarning;
        private readonly Action<string> _writeInformation;
        private readonly Action<string> _writeDebug;
        private readonly Action<string> _writeTrace;

        public virtual int Version => 0;

        public OperationLogHandler(
            Action<string> writeError = null,
            Action<string> writeWarning = null,
            Action<string> writeInformation = null,
            Action<string> writeDebug = null,
            Action<string> writeTrace = null)
        {
            _writeError = writeError;
            _writeWarning = writeWarning;
            _writeInformation = writeInformation;
            _writeDebug = writeDebug;
            _writeTrace = writeTrace;
        }

        public virtual void WriteError(string message)
            => _writeError?.Invoke(message);

        public virtual void WriteWarning(string message)
            => _writeWarning?.Invoke(message);

        public virtual void WriteInformation(string message)
            => _writeInformation?.Invoke(message);

        public virtual void WriteDebug(string message)
            => _writeDebug?.Invoke(message);

        public virtual void WriteTrace(string message)
            => _writeTrace?.Invoke(message);
    }
}

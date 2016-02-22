// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Design
{
    public interface IOperationResultHandler
    {
        int Version { get; }
        void OnResult(object value);
        void OnError(string type, string message, string stackTrace);
    }

    public interface IOperationLogHandler
    {
        int Version { get; }
        void WriteError(string message);
        void WriteWarning(string message);
        void WriteInformation(string message);
        void WriteDebug(string message);
        void WriteTrace(string message);
    }

    public partial class OperationResultHandler : IOperationResultHandler
    {
        private bool _hasResult;
        private object _result;
        private string _errorType;
        private string _errorMessage;
        private string _errorStackTrace;

        public virtual int Version => 0;

        public virtual bool HasResult => _hasResult;

        public virtual object Result => _result;

        public virtual string ErrorType => _errorType;

        public virtual string ErrorMessage => _errorMessage;

        public virtual string ErrorStackTrace => _errorStackTrace;

        public virtual void OnResult(object value)
        {
            _hasResult = true;
            _result = value;
        }

        public virtual void OnError(string type, string message, string stackTrace)
        {
            _errorType = type;
            _errorMessage = message;
            _errorStackTrace = stackTrace;
        }
    }

    public partial class OperationLogHandler : IOperationLogHandler
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

        public virtual void WriteError(string message) => _writeError?.Invoke(message);

        public virtual void WriteWarning(string message) => _writeWarning?.Invoke(message);

        public virtual void WriteInformation(string message) => _writeInformation?.Invoke(message);

        public virtual void WriteDebug(string message) => _writeDebug?.Invoke(message);

        public virtual void WriteTrace(string message) => _writeTrace?.Invoke(message);
    }

#if NET451
    partial class OperationResultHandler : MarshalByRefObject
    {
    }

    partial class OperationLogHandler : MarshalByRefObject
    {
    }
#endif
}

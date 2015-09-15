// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ENABLE_HANDLERS && !DNXCORE50
using System;
#endif

namespace Microsoft.Data.Entity.Design
{
#if !OMIT_HANDLER_INTERFACES
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
        void WriteVerbose(string message);
        void WriteDebug(string message);
    }
#endif

#if ENABLE_HANDLERS && !DNXCORE50
    public class OperationResultHandler : MarshalByRefObject, IOperationResultHandler
    {
        private bool _hasResult;
        private object _result;
        private string _errorType;
        private string _errorMessage;
        private string _errorStackTrace;

        public virtual int Version
        {
            get { return 0; }
        }

        public virtual bool HasResult
        {
            get { return _hasResult; }
        }

        public virtual object Result
        {
            get { return _result; }
        }

        public virtual string ErrorType
        {
            get { return _errorType; }
        }

        public virtual string ErrorMessage
        {
            get { return _errorMessage; }
        }

        public virtual string ErrorStackTrace
        {
            get { return _errorStackTrace; }
        }

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

    public class OperationLogHandler : MarshalByRefObject, IOperationLogHandler
    {
        private readonly Action<string> _writeError;
        private readonly Action<string> _writeWarning;
        private readonly Action<string> _writeInformation;
        private readonly Action<string> _writeVerbose;
        private readonly Action<string> _writeDebug;

        public virtual int Version
        {
            get { return 0; }
        }

        public OperationLogHandler(
            Action<string> writeError = null,
            Action<string> writeWarning = null,
            Action<string> writeInformation = null,
            Action<string> writeVerbose = null,
            Action<string> writeDebug = null)
        {
            _writeError = writeError;
            _writeWarning = writeWarning;
            _writeInformation = writeInformation;
            _writeVerbose = writeVerbose;
            _writeDebug = writeDebug;
        }

        public virtual void WriteError(string message)
        {
            if (_writeError != null)
            {
                _writeError(message);
            }
        }

        public virtual void WriteWarning(string message)
        {
            if (_writeWarning != null)
            {
                _writeWarning(message);
            }
        }

        public virtual void WriteInformation(string message)
        {
            if (_writeInformation != null)
            {
                _writeInformation(message);
            }
        }

        public virtual void WriteVerbose(string message)
        {
            if (_writeVerbose != null)
            {
                _writeVerbose(message);
            }
        }

        public virtual void WriteDebug(string message)
        {
            if (_writeDebug != null)
            {
                _writeDebug(message);
            }
        }
    }
#endif
}

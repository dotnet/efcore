// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ENABLE_HANDLERS
using System;
#endif

namespace Microsoft.Data.Entity.Commands
{
#if !OMIT_HANDLER_INTERFACES
    public interface IResultHandler
    {
        void OnResult(object value);
        void OnError(string type, string message, string stackTrace);
    }

    public interface ILogHandler
    {
        void WriteWarning(string message);
        void WriteInformation(string message);
        void WriteVerbose(string message);
    }
#endif

#if ENABLE_HANDLERS
    public class ResultHandler : MarshalByRefObject, IResultHandler
    {
        private bool _hasResult;
        private object _result;
        private string _errorType;
        private string _errorMessage;
        private string _errorStackTrace;

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

    public class LogHandler : MarshalByRefObject, ILogHandler
    {
        private readonly Action<string> _writeWarning;
        private readonly Action<string> _writeInformation;
        private readonly Action<string> _writeVerbose;

        public LogHandler(
            Action<string> writeWarning = null,
            Action<string> writeInformation = null,
            Action<string> writeVerbose = null)
        {
            _writeWarning = writeWarning;
            _writeInformation = writeInformation;
            _writeVerbose = writeVerbose;
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
    }
#endif
}

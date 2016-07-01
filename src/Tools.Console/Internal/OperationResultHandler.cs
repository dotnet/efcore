// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Design;

namespace Microsoft.EntityFrameworkCore.Tools.Internal
{
    public class OperationResultHandler : DesignMarshalByRefObject, IOperationResultHandler
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
}

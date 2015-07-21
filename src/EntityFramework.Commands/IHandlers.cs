// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Commands
{
    // TODO: Consider version resiliency
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
}

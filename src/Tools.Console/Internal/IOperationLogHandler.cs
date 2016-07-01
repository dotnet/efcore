// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Design
{
    public interface IOperationLogHandler
    {
        int Version { get; }
        void WriteError(string message);
        void WriteWarning(string message);
        void WriteInformation(string message);
        void WriteDebug(string message);
        void WriteTrace(string message);
    }
}

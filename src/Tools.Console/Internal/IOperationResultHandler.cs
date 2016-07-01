// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Design
{
    public interface IOperationResultHandler
    {
        int Version { get; }
        void OnResult(object value);
        void OnError(string type, string message, string stackTrace);
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design
{
    public interface IHandler
    {
        void OnResult(object value);
        void OnError(string type, string message, string stackTrace);
        void WriteError(string message);
        void WriteWarning(string message);
        void WriteInformation(string message);
        void WriteVerbose(string message);
    }
}

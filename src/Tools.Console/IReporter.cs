// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Tools
{
    public interface IReporter
    {
        bool SupportsColor { get; }
        void Verbose(string message);
        void Warning(string message);
        void Error(string message);
        void Output(string message);
    }
}

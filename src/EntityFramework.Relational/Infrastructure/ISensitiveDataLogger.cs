// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Infrastructure
{
    public interface ISensitiveDataLogger : ILogger
    {
        bool LogSensitiveData { get; }
    }
}

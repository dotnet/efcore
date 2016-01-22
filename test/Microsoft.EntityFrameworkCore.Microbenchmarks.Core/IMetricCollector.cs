// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Microbenchmarks.Core
{
    public interface IMetricCollector
    {
        IDisposable StartCollection();
        void StopCollection();
        void Reset();
        long TimeElapsed { get; }
        long MemoryDelta { get; }
    }
}

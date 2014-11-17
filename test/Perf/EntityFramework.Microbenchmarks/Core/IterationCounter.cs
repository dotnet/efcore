// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace EntityFramework.Microbenchmarks.Core
{
    public class IterationCounterBase
    {
        public long WorkingSet { get; set; }
    }

    public class IterationCounter : IterationCounterBase
    {
        public long ElapsedMillis { get; set; }
    }

    public class ThreadedIterationCounter : IterationCounterBase
    {
        public long RequestsPerSecond { get; set; }
    }
}

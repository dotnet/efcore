// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace EntityFramework.Microbenchmarks.Core
{
    public class IterationCounter
    {
        public long WorkingSet { get; set; }
        public long ElapsedMillis { get; set; }
    }
}

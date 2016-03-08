// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Microbenchmarks.Core
{
#if NET451
    public partial class MetricCollector : MarshalByRefObject
    {
        private partial class Scope : MarshalByRefObject
        {
        }
    }
#endif

    public partial class MetricCollector : IMetricCollector
    {
        private bool _collecting;
        private readonly Scope _scope;
        private readonly Stopwatch _timer = new Stopwatch();
        private long _memoryOnCurrentCollectionStarted;

        public MetricCollector()
        {
            _scope = new Scope(this);
        }

        public IDisposable StartCollection()
        {
            _collecting = true;
            _memoryOnCurrentCollectionStarted = GetCurrentMemory();
            _timer.Start();
            return _scope;
        }

        public void StopCollection()
        {
            if (_collecting)
            {
                _timer.Stop();
                _collecting = false;
                var currentMemory = GetCurrentMemory();
                MemoryDelta += currentMemory - _memoryOnCurrentCollectionStarted;
            }
        }

        public void Reset()
        {
            _collecting = false;
            _timer.Reset();
            MemoryDelta = 0;
        }

        public long TimeElapsed => _timer.ElapsedMilliseconds;

        public long MemoryDelta { get; private set; }

        private static long GetCurrentMemory()
        {
            for (var i = 0; i < 5; i++)
            {
                GC.GetTotalMemory(forceFullCollection: true);
            }

            return GC.GetTotalMemory(forceFullCollection: true);
        }

        private partial class Scope : IDisposable
        {
            private readonly IMetricCollector _collector;

            public Scope(IMetricCollector collector)
            {
                _collector = collector;
            }

            public void Dispose()
            {
                _collector.StopCollection();
            }
        }
    }
}

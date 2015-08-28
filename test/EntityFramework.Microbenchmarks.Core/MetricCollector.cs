// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace EntityFramework.Microbenchmarks.Core
{
#if !DNXCORE50
    public partial class MetricCollector : MarshalByRefObject
    {
        private partial class Scope : MarshalByRefObject
        {
        }
    }
#endif

    public partial class MetricCollector 
    {
        private bool _collecting;
        private readonly Scope _scope;
        private Stopwatch _timer = new Stopwatch();
        private long _cumulativeMemoryDelta;
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
                _cumulativeMemoryDelta += (currentMemory - _memoryOnCurrentCollectionStarted);
            }
        }

        public void Reset()
        {
            _collecting = false;
            _timer.Reset();
            _cumulativeMemoryDelta = 0;
        }

        public long TimeElapsed => _timer.ElapsedMilliseconds;

        public long MemoryDelta => _cumulativeMemoryDelta;

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
            private readonly MetricCollector _collector;

            public Scope(MetricCollector collector)
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

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Benchmarks
{
#if NET452
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
        private readonly Stopwatch _timer = new Stopwatch();
        private long _memoryOnCurrentCollectionStarted;

        public IDisposable StartCollection()
        {
            if (_collecting)
            {
                throw new InvalidOperationException("Collection already started.");
            }

            _collecting = true;
            _memoryOnCurrentCollectionStarted = GetCurrentMemory();
            _timer.Restart();

            return new Scope(this);
        }

        public void StopCollection()
        {
            if (!_collecting)
            {
                throw new InvalidOperationException("Collection not started.");
            }

            _timer.Stop();
            _collecting = false;

            MemoryDelta += GetCurrentMemory() - _memoryOnCurrentCollectionStarted;
            TimeElapsed += _timer.ElapsedMilliseconds;
        }

        public void Reset()
        {
            if (_collecting)
            {
                throw new InvalidOperationException("Can not reset while collecting.");
            }

            MemoryDelta = 0;
            TimeElapsed = 0;
        }

        public long TimeElapsed { get; private set; }
        public long MemoryDelta { get; private set; }

        private static long GetCurrentMemory()
        {
            for (var i = 0; i < 5; i++)
            {
                GC.GetTotalMemory(forceFullCollection: true);
            }

            return GC.GetTotalMemory(forceFullCollection: true);
        }

        void IXunitSerializable.Deserialize(IXunitSerializationInfo info)
        {
            TimeElapsed = info.GetValue<long>(nameof(TimeElapsed));
            MemoryDelta = info.GetValue<long>(nameof(MemoryDelta));
        }

        void IXunitSerializable.Serialize(IXunitSerializationInfo info)
        {
            if(_collecting)
            {
                throw new InvalidOperationException("Can not serialize while collection is running.");
            }

            info.AddValue(nameof(TimeElapsed), TimeElapsed);
            info.AddValue(nameof(MemoryDelta), MemoryDelta);
        }

        private partial class Scope : IDisposable
        {
            private readonly IMetricCollector _collector;

            public Scope(IMetricCollector collector)
            {
                _collector = collector;
            }

            public void Dispose() => _collector.StopCollection();
        }
    }
}

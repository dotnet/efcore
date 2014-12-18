// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace EntityFramework.Microbenchmarks.Core
{
    public class MetricCollector
    {
        private readonly Stopwatch _iterationStopwatch;
        private readonly Stopwatch _runStopwatch;
        private readonly Scope _scope;

        public MetricCollector(Stopwatch iterationStopwatch, Stopwatch runStopwatch)
        {
            _iterationStopwatch = iterationStopwatch;
            _runStopwatch = runStopwatch;
            _scope = new Scope(this);
        }

        public IDisposable Start()
        {
            _iterationStopwatch.Start();
            _runStopwatch.Start();
            return _scope; 
        }

        public void Stop()
        {
            _iterationStopwatch.Stop();
            _runStopwatch.Stop();
        }

        private class Scope : IDisposable
        {
            private readonly MetricCollector _collector;

            public Scope(MetricCollector collector)
            {
                _collector = collector;
            }

            public void Dispose()
            {
                _collector.Stop();
            }
        }
    }
}

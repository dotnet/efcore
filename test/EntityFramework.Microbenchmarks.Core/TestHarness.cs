// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace EntityFramework.Microbenchmarks.Core
{
    public class TestHarness
    {
        private readonly Stopwatch _iterationStopwatch;
        private readonly Stopwatch _runStopwatch;
        private readonly Scope _collectionScope;

        public static TestHarness NullHarness { get; } = new NullTestHarness();

        private TestHarness()
        {
            _collectionScope = new Scope(this);
        }

        public TestHarness(Stopwatch iterationStopwatch, Stopwatch runStopwatch)
        {
            _iterationStopwatch = iterationStopwatch;
            _runStopwatch = runStopwatch;
            _collectionScope = new Scope(this);
        }

        protected IDisposable CollectionScope
        {
            get { return _collectionScope; }
        }

        public virtual IDisposable StartCollection()
        {
            _iterationStopwatch.Start();
            _runStopwatch.Start();
            return _collectionScope;
        }

        public virtual void StopCollection()
        {
            _iterationStopwatch.Stop();
            _runStopwatch.Stop();
        }

        private class Scope : IDisposable
        {
            private readonly TestHarness _harness;

            public Scope(TestHarness harness)
            {
                _harness = harness;
            }

            public void Dispose()
            {
                _harness.StopCollection();
            }
        }

        private class NullTestHarness : TestHarness
        {
            public override IDisposable StartCollection()
            {
                return CollectionScope;
            }

            public override void StopCollection()
            {
            }
        }
    }
}

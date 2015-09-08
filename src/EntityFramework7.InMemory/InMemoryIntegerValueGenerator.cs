// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryIntegerValueGenerator<TValue> : ValueGenerator<TValue>
    {
        private long _current;

        public override TValue Next() => (TValue)Convert.ChangeType(Interlocked.Increment(ref _current), typeof(TValue));

        public override bool GeneratesTemporaryValues => false;
    }
}

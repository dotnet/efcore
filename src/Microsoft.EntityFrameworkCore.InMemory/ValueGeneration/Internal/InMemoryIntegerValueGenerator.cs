// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore.ValueGeneration.Internal
{
    public class InMemoryIntegerValueGenerator<TValue> : ValueGenerator<TValue>
    {
        private long _current;

        public override TValue Next(EntityEntry entry) 
            => (TValue)Convert.ChangeType(Interlocked.Increment(ref _current), typeof(TValue));

        public override bool GeneratesTemporaryValues => false;
    }
}

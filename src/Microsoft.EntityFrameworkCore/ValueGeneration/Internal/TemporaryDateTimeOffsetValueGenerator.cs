// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore.ValueGeneration.Internal
{
    public class TemporaryDateTimeOffsetValueGenerator : ValueGenerator<DateTimeOffset>
    {
        private long _current;

        public override DateTimeOffset Next(EntityEntry entry)
            => new DateTimeOffset(Interlocked.Increment(ref _current), TimeSpan.Zero);

        public override bool GeneratesTemporaryValues => true;
    }
}

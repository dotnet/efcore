// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore.ValueGeneration.Internal
{
    public class TemporaryDecimalValueGenerator : TemporaryNumberValueGenerator<decimal>
    {
        private int _current = int.MinValue + 1000;

        public override decimal Next(EntityEntry entry) => Interlocked.Increment(ref _current);
    }
}

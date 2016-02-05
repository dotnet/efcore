// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;

namespace Microsoft.EntityFrameworkCore.ValueGeneration.Internal
{
    public class TemporaryLongValueGenerator : TemporaryNumberValueGenerator<long>
    {
        private long _current = long.MinValue + 1000;

        public override long Next() => Interlocked.Increment(ref _current);
    }
}

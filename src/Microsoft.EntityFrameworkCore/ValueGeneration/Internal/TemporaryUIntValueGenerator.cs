// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;

namespace Microsoft.EntityFrameworkCore.ValueGeneration.Internal
{
    public class TemporaryUIntValueGenerator : TemporaryNumberValueGenerator<uint>
    {
        private int _current = int.MinValue + 1000;

        public override uint Next() => (uint)Interlocked.Increment(ref _current);
    }
}

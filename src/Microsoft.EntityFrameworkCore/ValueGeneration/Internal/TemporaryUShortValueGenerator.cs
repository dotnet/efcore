// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;

namespace Microsoft.EntityFrameworkCore.ValueGeneration.Internal
{
    public class TemporaryUShortValueGenerator : TemporaryNumberValueGenerator<ushort>
    {
        private int _current = short.MinValue + 100;

        public override ushort Next() => (ushort)Interlocked.Increment(ref _current);
    }
}

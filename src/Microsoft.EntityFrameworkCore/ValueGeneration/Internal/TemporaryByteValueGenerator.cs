// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore.ValueGeneration.Internal
{
    public class TemporaryByteValueGenerator : TemporaryNumberValueGenerator<byte>
    {
        private int _current;

        public override byte Next(EntityEntry entry) => (byte)Interlocked.Decrement(ref _current);
    }
}

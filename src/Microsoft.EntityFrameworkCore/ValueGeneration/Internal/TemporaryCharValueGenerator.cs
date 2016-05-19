// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore.ValueGeneration.Internal
{
    public class TemporaryCharValueGenerator : TemporaryNumberValueGenerator<char>
    {
        private int _current = char.MaxValue - 100;

        public override char Next(EntityEntry entry) => (char)Interlocked.Decrement(ref _current);
    }
}

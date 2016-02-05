// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;

namespace Microsoft.EntityFrameworkCore.ValueGeneration.Internal
{
    public class TemporarySByteValueGenerator : TemporaryNumberValueGenerator<sbyte>
    {
        private int _current = sbyte.MinValue;

        public override sbyte Next() => (sbyte)Interlocked.Increment(ref _current);
    }
}

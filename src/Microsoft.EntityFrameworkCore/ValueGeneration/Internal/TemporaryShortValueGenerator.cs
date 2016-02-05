// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;

namespace Microsoft.EntityFrameworkCore.ValueGeneration.Internal
{
    public class TemporaryShortValueGenerator : TemporaryNumberValueGenerator<short>
    {
        private int _current = short.MinValue + 100;

        public override short Next() => (short)Interlocked.Increment(ref _current);
    }
}

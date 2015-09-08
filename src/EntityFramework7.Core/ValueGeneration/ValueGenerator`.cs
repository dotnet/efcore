// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.ValueGeneration
{
    public abstract class ValueGenerator<TValue> : ValueGenerator
    {
        public new abstract TValue Next();

        protected override object NextValue() => Next();
    }
}

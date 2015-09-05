// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Entity.ValueGeneration.Internal
{
    public class TemporaryStringValueGenerator : ValueGenerator<string>
    {
        public override string Next() => Guid.NewGuid().ToString();

        public override bool GeneratesTemporaryValues => true;
    }
}

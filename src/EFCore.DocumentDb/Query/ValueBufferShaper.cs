// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ValueBufferShaper : Shaper, IShaper<ValueBuffer>
    {
        public ValueBuffer Shape(QueryContext queryContext, ValueBuffer valueBuffer) => valueBuffer;

        public override Type Type => typeof(ValueBuffer);
    }
}

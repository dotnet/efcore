// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public class ValueBufferShaper : Shaper, IShaper<ValueBuffer>
    {
        public ValueBufferShaper([NotNull] IQuerySource querySource)
            : base(querySource)
        {
        }

        public override Type Type => typeof(ValueBuffer);

        public virtual ValueBuffer Shape(QueryContext queryContext, ValueBuffer valueBuffer)
            => valueBuffer;
    }
}

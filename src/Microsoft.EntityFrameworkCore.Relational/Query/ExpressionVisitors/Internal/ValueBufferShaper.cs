// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    public class ValueBufferShaper : Shaper, IShaper<ValueBuffer>
    {
        public ValueBufferShaper([NotNull] IQuerySource querySource)
            : base(querySource)
        {
        }

        public override Type Type => typeof(ValueBuffer);

        public virtual object GetKey(QueryContext queryContext, ValueBuffer valueBuffer)
            => null;

        public virtual ValueBuffer Shape(QueryContext queryContext, ValueBuffer valueBuffer)
            => valueBuffer;
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public abstract class EntityShaper : Shaper
    {
        protected EntityShaper([NotNull] IQuerySource querySource)
            : base(querySource)
        {
        }

        public virtual bool AllowNullResult { get; set; }
    }
}

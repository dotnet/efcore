// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public class InMemoryGroupByShaperExpression : GroupByShaperExpression
    {
        public InMemoryGroupByShaperExpression(
            [NotNull] Expression keySelector,
            [NotNull] Expression elementSelector,
            [NotNull] ParameterExpression groupingParameter,
            [NotNull] ParameterExpression valueBufferParameter)
            : base(keySelector, elementSelector)
        {
            GroupingParameter = groupingParameter;
            ValueBufferParameter = valueBufferParameter;
        }

        public virtual ParameterExpression GroupingParameter { get; }
        public virtual ParameterExpression ValueBufferParameter { get; }
    }
}

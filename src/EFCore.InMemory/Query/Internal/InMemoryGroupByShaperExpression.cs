// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public class InMemoryGroupByShaperExpression : GroupByShaperExpression
    {
        public InMemoryGroupByShaperExpression(
            Expression keySelector,
            Expression elementSelector,
            ParameterExpression groupingParameter,
            ParameterExpression valueBufferParameter)
            : base(keySelector, elementSelector)
        {
            GroupingParameter = groupingParameter;
            ValueBufferParameter = valueBufferParameter;
        }

        public virtual ParameterExpression GroupingParameter { get; }
        public virtual ParameterExpression ValueBufferParameter { get; }
    }
}

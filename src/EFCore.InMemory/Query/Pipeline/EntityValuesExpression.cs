// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Pipeline
{
    public class EntityProjectionExpression : Expression
    {
        public EntityProjectionExpression(IEntityType entityType, int startIndex)
        {
            EntityType = entityType;
            StartIndex = startIndex;
        }

        public IEntityType EntityType { get; }
        public int StartIndex { get; }
    }

}

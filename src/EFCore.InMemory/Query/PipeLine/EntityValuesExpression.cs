// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Pipeline
{
    public class EntityValuesExpression : Expression
    {
        public EntityValuesExpression(int startIndex)
        {
            StartIndex = startIndex;
        }

        public int StartIndex { get; }
    }

}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.Data.Entity.Relational.Query.Methods
{
    public class CompositeMethodCallTranslator : IMethodCallTranslator
    {
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            return new EqualsTranslator().Translate(methodCallExpression)
                   ?? new StartsWithTranslator().Translate(methodCallExpression)
                   ?? new EndsWithTranslator().Translate(methodCallExpression);
        }
    }
}

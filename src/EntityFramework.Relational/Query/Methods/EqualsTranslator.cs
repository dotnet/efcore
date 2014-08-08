// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.Data.Entity.Relational.Query.Methods
{
    public class EqualsTranslator : IMethodCallTranslator
    {
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            return methodCallExpression.Method.Name == "Equals"
                   && methodCallExpression.Arguments.Count == 1
                ? Expression.Equal(methodCallExpression.Object, methodCallExpression.Arguments[0])
                : null;
        }
    }
}

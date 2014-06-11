// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Query.Methods
{
    public interface IMethodCallTranslator
    {
        Expression Translate([NotNull] MethodCallExpression expression);
    }
}

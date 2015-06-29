// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.Data.Entity.Query
{
    public interface IQueryMethodProvider
    {
        MethodInfo ShapedQueryMethod { get; }
        MethodInfo QueryMethod { get; }
        MethodInfo GetResultMethod { get; }
        MethodInfo IncludeMethod { get; }
        MethodInfo CreateReferenceIncludeRelatedValuesStrategyMethod { get; }
        MethodInfo CreateCollectionIncludeRelatedValuesStrategyMethod { get; }
        Type IncludeRelatedValuesFactoryType { get; }
    }
}

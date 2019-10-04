// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    public interface IShaper
    {
        LambdaExpression CreateShaperLambda();
        Type Type { get; }
    }
}

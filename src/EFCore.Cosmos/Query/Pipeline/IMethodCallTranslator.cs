// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public interface IMethodCallTranslator
    {
        SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments);
    }
}

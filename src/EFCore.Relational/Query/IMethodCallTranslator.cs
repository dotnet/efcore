// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public interface IMethodCallTranslator
    {
        SqlExpression Translate(
            [CanBeNull] SqlExpression instance, [NotNull] MethodInfo method, [NotNull] IReadOnlyList<SqlExpression> arguments);
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public interface IMethodCallTranslatorProvider
    {
        SqlExpression Translate(IModel model, SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments);
    }
}

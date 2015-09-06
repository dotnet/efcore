// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;

namespace Microsoft.Data.Entity.Query.Sql
{
    public interface ISqlQueryGeneratorFactory
    {
        ISqlQueryGenerator CreateGenerator([NotNull] SelectExpression selectExpression);
        ISqlQueryGenerator CreateRawCommandGenerator(
            [NotNull] SelectExpression selectExpression,
            [NotNull] string sql,
            [NotNull] object[] parameters);
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Query.Sql
{
    public interface ISqlGeneratingExpressionTreeVisitorFactory
    {
        SqlGeneratingExpressionTreeVisitor Create(
            [NotNull] StringBuilder sql,
            [NotNull] IParameterFactory parameterFactory);
    }
}

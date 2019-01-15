// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerNewGuidTranslator : IMethodCallTranslator
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private static MethodInfo _methodInfo = typeof(Guid).GetRuntimeMethod(nameof(Guid.NewGuid), Array.Empty<Type>());

        public SqlServerNewGuidTranslator(IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            return _methodInfo.Equals(method)
                ? new SqlFunctionExpression(
                    "NEWID",
                    null,
                    method.ReturnType,
                    _typeMappingSource.FindMapping(method.ReturnType),
                    false)
                : null;
        }
    }
}

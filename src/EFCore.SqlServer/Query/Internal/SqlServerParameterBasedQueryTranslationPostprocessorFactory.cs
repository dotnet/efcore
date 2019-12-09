// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerParameterBasedQueryTranslationPostprocessorFactory : IRelationalParameterBasedQueryTranslationPostprocessorFactory
    {
        private readonly RelationalParameterBasedQueryTranslationPostprocessorDependencies _dependencies;

        public SqlServerParameterBasedQueryTranslationPostprocessorFactory(
            [NotNull] RelationalParameterBasedQueryTranslationPostprocessorDependencies dependencies)
        {
            _dependencies = dependencies;
        }

        public virtual RelationalParameterBasedQueryTranslationPostprocessor Create(bool useRelationalNulls)
            => new SqlServerParameterBasedQueryTranslationPostprocessor(_dependencies, useRelationalNulls);
    }
}

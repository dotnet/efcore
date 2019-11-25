// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query
{
    public class RelationalParameterBasedQueryTranslationPostprocessorFactory : IRelationalParameterBasedQueryTranslationPostprocessorFactory
    {
        private readonly RelationalParameterBasedQueryTranslationPostprocessorDependencies _dependencies;

        public RelationalParameterBasedQueryTranslationPostprocessorFactory(RelationalParameterBasedQueryTranslationPostprocessorDependencies dependencies)
        {
            _dependencies = dependencies;
        }

        public virtual RelationalParameterBasedQueryTranslationPostprocessor Create(bool useRelationalNulls)
            => new RelationalParameterBasedQueryTranslationPostprocessor(_dependencies, useRelationalNulls);
    }
}

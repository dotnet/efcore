// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class RelationalParameterBasedQueryTranslationPostprocessorFactory : IRelationalParameterBasedQueryTranslationPostprocessorFactory
    {
        private readonly RelationalParameterBasedQueryTranslationPostprocessorDependencies _dependencies;

        public RelationalParameterBasedQueryTranslationPostprocessorFactory(
            [NotNull] RelationalParameterBasedQueryTranslationPostprocessorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            _dependencies = dependencies;
        }

        public virtual RelationalParameterBasedQueryTranslationPostprocessor Create(bool useRelationalNulls)
            => new RelationalParameterBasedQueryTranslationPostprocessor(_dependencies, useRelationalNulls);
    }
}

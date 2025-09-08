// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGParametersBasedSqlProcessorFactory : IRelationalParameterBasedSqlProcessorFactory
    {
        private readonly RelationalParameterBasedSqlProcessorDependencies _dependencies;
        [NotNull] private readonly IXGOptions _options;

        public XGParametersBasedSqlProcessorFactory(
            [NotNull] RelationalParameterBasedSqlProcessorDependencies dependencies,
            [NotNull] IXGOptions options)
        {
            _dependencies = dependencies;
            _options = options;
        }

        public virtual RelationalParameterBasedSqlProcessor Create(RelationalParameterBasedSqlProcessorParameters parameters)
            => new XGParametersBasedSqlProcessor(_dependencies, parameters, _options);
    }
}

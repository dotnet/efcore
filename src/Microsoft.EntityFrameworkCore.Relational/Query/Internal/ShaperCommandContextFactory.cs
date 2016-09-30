// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ShaperCommandContextFactory : IShaperCommandContextFactory
    {
        private readonly IRelationalValueBufferFactoryFactory _valueBufferFactoryFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ShaperCommandContextFactory([NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory)
        {
            Check.NotNull(valueBufferFactoryFactory, nameof(valueBufferFactoryFactory));

            _valueBufferFactoryFactory = valueBufferFactoryFactory;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ShaperCommandContext Create(Func<IQuerySqlGenerator> sqlGeneratorFunc)
            => new ShaperCommandContext(
                _valueBufferFactoryFactory,
                sqlGeneratorFunc);
    }
}

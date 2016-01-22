// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class ShaperCommandContextFactory : IShaperCommandContextFactory
    {
        private readonly IRelationalValueBufferFactoryFactory _valueBufferFactoryFactory;

        public ShaperCommandContextFactory([NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory)
        {
            Check.NotNull(valueBufferFactoryFactory, nameof(valueBufferFactoryFactory));

            _valueBufferFactoryFactory = valueBufferFactoryFactory;
        }

        public virtual ShaperCommandContext Create(Func<IQuerySqlGenerator> sqlGeneratorFunc)
            => new ShaperCommandContext(
                _valueBufferFactoryFactory,
                sqlGeneratorFunc);
    }
}

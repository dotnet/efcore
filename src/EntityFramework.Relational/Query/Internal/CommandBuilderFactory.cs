// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class CommandBuilderFactory : ICommandBuilderFactory
    {
        private readonly IRelationalValueBufferFactoryFactory _valueBufferFactoryFactory;

        public CommandBuilderFactory([NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory)
        {
            Check.NotNull(valueBufferFactoryFactory, nameof(valueBufferFactoryFactory));

            _valueBufferFactoryFactory = valueBufferFactoryFactory;
        }

        public virtual CommandBuilder Create(Func<ISqlQueryGenerator> sqlGeneratorFunc)
            => new CommandBuilder(
                _valueBufferFactoryFactory,
                sqlGeneratorFunc);
    }
}

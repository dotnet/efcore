// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class CommandBuilderFactory : ICommandBuilderFactory
    {
        private readonly IRelationalValueBufferFactoryFactory _valueBufferFactoryFactory;
        private readonly IRelationalTypeMapper _typeMapper;

        public CommandBuilderFactory(
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
            [NotNull] IRelationalTypeMapper typeMapper)
        {
            _valueBufferFactoryFactory = valueBufferFactoryFactory;
            _typeMapper = typeMapper;
        }

        public virtual CommandBuilder Create(Func<ISqlQueryGenerator> sqlGeneratorFunc)
            => new CommandBuilder(
                _valueBufferFactoryFactory,
                _typeMapper,
                sqlGeneratorFunc);
    }
}

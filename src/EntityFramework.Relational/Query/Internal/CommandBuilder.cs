// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class CommandBuilder
    {
        private readonly IRelationalValueBufferFactoryFactory _valueBufferFactoryFactory;
        private readonly IRelationalTypeMapper _typeMapper;
        private readonly Func<ISqlQueryGenerator> _sqlGeneratorFunc;

        private IRelationalValueBufferFactory _valueBufferFactory;

        public CommandBuilder(
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] Func<ISqlQueryGenerator> sqlGeneratorFunc)
        {
            _valueBufferFactoryFactory = valueBufferFactoryFactory;
            _typeMapper = typeMapper;
            _sqlGeneratorFunc = sqlGeneratorFunc;
        }

        public virtual IRelationalValueBufferFactory ValueBufferFactory => _valueBufferFactory;

        public virtual Func<ISqlQueryGenerator> SqlGeneratorFunc => _sqlGeneratorFunc;

        public virtual DbCommand Build(
            [NotNull] IRelationalConnection connection,
            [NotNull] IDictionary<string, object> parameterValues)
            => _sqlGeneratorFunc()
                .GenerateSql(parameterValues)
                .CreateDbCommand(connection, _typeMapper);

        public virtual void NotifyReaderCreated([NotNull] DbDataReader dataReader)
            => LazyInitializer
                .EnsureInitialized(
                    ref _valueBufferFactory,
                    () => _sqlGeneratorFunc()
                        .CreateValueBufferFactory(_valueBufferFactoryFactory, dataReader));
    }
}

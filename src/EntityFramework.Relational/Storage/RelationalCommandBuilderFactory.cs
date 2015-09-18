// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Storage
{
    public class RelationalCommandBuilderFactory : IRelationalCommandBuilderFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IRelationalTypeMapper _typeMapper;

        public RelationalCommandBuilderFactory(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(loggerFactory, nameof(loggerFactory));
            Check.NotNull(typeMapper, nameof(typeMapper));

            _loggerFactory = loggerFactory;
            _typeMapper = typeMapper;
        }

        public virtual IRelationalCommandBuilder Create()
            => new RelationalCommandBuilder(
                _loggerFactory,
                _typeMapper);
    }
}

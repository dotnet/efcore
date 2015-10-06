// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class RelationalCommandBuilderFactory : IRelationalCommandBuilderFactory
    {
        private readonly IRelationalTypeMapper _typeMapper;

        public RelationalCommandBuilderFactory([NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));

            _typeMapper = typeMapper;
        }

        public virtual IRelationalCommandBuilder Create()
            => new RelationalCommandBuilder(_typeMapper);
    }
}

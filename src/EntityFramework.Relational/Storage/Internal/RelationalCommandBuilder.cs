// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class RelationalCommandBuilder : IRelationalCommandBuilder
    {
        private readonly IRelationalTypeMapper _typeMapper;
        private readonly List<RelationalParameter> _parameters = new List<RelationalParameter>();

        public RelationalCommandBuilder([NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));

            _typeMapper = typeMapper;
        }

        public virtual IndentedStringBuilder CommandTextBuilder { get; } = new IndentedStringBuilder();

        public virtual IRelationalCommandBuilder AddParameter(
            [NotNull] string name,
            [CanBeNull] object value,
            [NotNull] Func<IRelationalTypeMapper, RelationalTypeMapping> mapType,
            bool? nullable)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(mapType, nameof(mapType));

            _parameters.Add(
                new RelationalParameter(
                    name,
                    value,
                    mapType(_typeMapper),
                    nullable));

            return this;
        }

        public virtual IRelationalCommand BuildRelationalCommand()
                => new RelationalCommand(
                    CommandTextBuilder.ToString(),
                    _parameters);

        public override string ToString() => CommandTextBuilder.ToString();
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class RelationalParameter : IRelationalParameter
    {
        private readonly string _name;
        private readonly RelationalTypeMapping _relationalTypeMapping;
        private readonly bool? _nullable;

        public RelationalParameter(
            [NotNull] string invariantName,
            [NotNull] string name,
            [NotNull] RelationalTypeMapping relationalTypeMapping,
            [CanBeNull] bool? nullable)
        {
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(relationalTypeMapping, nameof(relationalTypeMapping));

            InvariantName = invariantName;
            _name = name;
            _relationalTypeMapping = relationalTypeMapping;
            _nullable = nullable;
        }

        public virtual string InvariantName { get; }

        public virtual RelationalTypeMapping RelationalTypeMapping => _relationalTypeMapping;

        public virtual bool? Nullable => _nullable;

        public virtual void AddDbParameter(DbCommand command, object value)
        {
            Check.NotNull(command, nameof(command));

            command.Parameters
                .Add(_relationalTypeMapping
                    .CreateParameter(command, _name, value, _nullable));
        }
    }
}

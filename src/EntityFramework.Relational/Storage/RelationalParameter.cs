// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class RelationalParameter : IRelationalParameter
    {
        private readonly string _name;
        private readonly object _value;
        private readonly RelationalTypeMapping _relationalTypeMapping;
        private readonly bool? _nullable;

        public RelationalParameter(
            [NotNull] string name,
            [CanBeNull] object value,
            [NotNull] RelationalTypeMapping relationalTypeMapping,
            [CanBeNull] bool? nullable,
            [CanBeNull] string invariantName)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(relationalTypeMapping, nameof(relationalTypeMapping));

            _name = name;
            _value = value;
            _relationalTypeMapping = relationalTypeMapping;
            _nullable = nullable;

            InvariantName = invariantName;
        }

        public virtual string InvariantName { get; }

        public virtual void AddDbParameter(DbCommand command, object value)
        {
            Check.NotNull(command, nameof(command));

            command.Parameters
                .Add(_relationalTypeMapping
                    .CreateParameter(command, _name, value ?? _value, _nullable));
        }
    }
}

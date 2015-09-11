// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class RelationalParameter
    {
        private readonly IProperty _property;

        public RelationalParameter(
            [NotNull] string name,
            [CanBeNull] object value,
            [CanBeNull] IProperty property = null)
        {
            Check.NotNull(name, nameof(name));

            Name = name;
            Value = value;
            _property = property;
        }

        public virtual string Name { get; }

        public virtual object Value { get; }

        public virtual DbParameter CreateDbParameter(
            [NotNull] DbCommand command,
            [NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(command, nameof(command));

            return _property == null
                ? typeMapper.GetMapping(Value)
                    .CreateParameter(
                        command,
                        Name,
                        Value ?? DBNull.Value,
                        Value.GetType().IsNullableType())
                : typeMapper.GetMapping(_property)
                    .CreateParameter(
                        command,
                        Name,
                        Value ?? DBNull.Value,
                        _property.IsNullable);
        }
    }
}

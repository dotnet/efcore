// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class DynamicRelationalParameter : IRelationalParameter
    {
        private readonly IRelationalTypeMapper _typeMapper;

        public DynamicRelationalParameter(
            [NotNull] string invariantName,
            [NotNull] string name,
            [NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(typeMapper, nameof(typeMapper));

            InvariantName = invariantName;
            Name = name;
            _typeMapper = typeMapper;
        }

        public virtual string InvariantName { get; }

        public virtual string Name { get; }

        public virtual void AddDbParameter(DbCommand command, object value)
        {
            Check.NotNull(command, nameof(command));

            if (value == null)
            {
                command.Parameters
                    .Add(_typeMapper.GetMappingForValue(null)
                        .CreateParameter(command, Name, null));

                return;
            }

            var dbParameter = value as DbParameter;

            if (dbParameter != null)
            {
                command.Parameters.Add(dbParameter);

                return;
            }

            var type = value.GetType();

            command.Parameters
                .Add(_typeMapper.GetMapping(type)
                    .CreateParameter(command, Name, value, type.IsNullableType()));
        }
    }
}

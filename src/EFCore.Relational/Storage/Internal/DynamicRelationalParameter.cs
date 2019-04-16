// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DynamicRelationalParameter : RelationalParameterBase
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DynamicRelationalParameter(
            [NotNull] string invariantName,
            [NotNull] string name,
            [NotNull] IRelationalTypeMappingSource typeMappingSource)
        {
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));

            InvariantName = invariantName;
            Name = name;
            _typeMappingSource = typeMappingSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string InvariantName { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Name { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void AddDbParameter(DbCommand command, object value)
        {
            Check.NotNull(command, nameof(command));

            if (value == null)
            {
                command.Parameters
                    .Add(
                        _typeMappingSource.GetMappingForValue(null)
                            .CreateParameter(command, Name, null));

                return;
            }

            if (value is DbParameter dbParameter)
            {
                command.Parameters.Add(dbParameter);

                return;
            }

            var type = value.GetType();

            command.Parameters.Add(
                _typeMappingSource.GetMapping(type)
                    .CreateParameter(command, Name, value, type.IsNullableType()));
        }
    }
}

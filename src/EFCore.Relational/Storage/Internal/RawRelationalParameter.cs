// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RawRelationalParameter : RelationalParameterBase
    {
        private readonly DbParameter _parameter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RawRelationalParameter(
            [NotNull] string invariantName,
            [NotNull] DbParameter parameter)
        {
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotNull(parameter, nameof(parameter));

            InvariantName = invariantName;
            _parameter = parameter;
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
        public override void AddDbParameter(DbCommand command, IReadOnlyDictionary<string, object> parameterValues)
        {
            AddDbParameter(command, _parameter);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void AddDbParameter(DbCommand command, object value)
        {
            command.Parameters.Add(value);
        }
    }
}

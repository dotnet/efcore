// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class RelationalParameterBase : IRelationalParameter
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string InvariantName { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public abstract void AddDbParameter(DbCommand command, object value);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddDbParameter(DbCommand command, IReadOnlyDictionary<string, object> parameterValues)
        {
            Check.NotNull(command, nameof(command));
            Check.NotNull(parameterValues, nameof(parameterValues));

            if (parameterValues.TryGetValue(InvariantName, out var parameterValue))
            {
                AddDbParameter(command, parameterValue);
            }
            else
            {
                throw new InvalidOperationException(
                    RelationalStrings.MissingParameterValue(InvariantName));
            }
        }
    }
}

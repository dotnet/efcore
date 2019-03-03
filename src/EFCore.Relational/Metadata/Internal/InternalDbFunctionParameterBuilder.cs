// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InternalDbFunctionParameterBuilder
    {
        private readonly DbFunctionParameter _dbFunctionParameter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IMutableDbFunctionParameter Parameter => this._dbFunctionParameter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalDbFunctionParameterBuilder([NotNull] DbFunctionParameter dbFunctionParameter)
        {
            _dbFunctionParameter = dbFunctionParameter;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalDbFunctionParameterBuilder HasNullabilityPropagation(bool supportsNullabilityPropagation)
        {
            _dbFunctionParameter.SupportsNullabilityPropagation = supportsNullabilityPropagation;

            return this;
        }
    }
}

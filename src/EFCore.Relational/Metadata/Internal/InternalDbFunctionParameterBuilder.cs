// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

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
        public virtual DbFunctionParameter Parameter => _dbFunctionParameter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalDbFunctionParameterBuilder([NotNull] DbFunctionParameter dbFunctionParameter)
        {
            Check.NotNull(dbFunctionParameter, nameof(dbFunctionParameter));

            _dbFunctionParameter = dbFunctionParameter;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalDbFunctionParameterBuilder HasName([NotNull]string name, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(_dbFunctionParameter.GetNameConfigurationSource())
                || _dbFunctionParameter.Name == name)
            {
                _dbFunctionParameter.SetName(name, configurationSource);
            }

            return this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalDbFunctionParameterBuilder HasType([NotNull] Type type, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(_dbFunctionParameter.GetParameterTypeConfigurationSource())
                || _dbFunctionParameter.ParameterType == type)
            {
                _dbFunctionParameter.SetParameterType(type, configurationSource);
            }

            return this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalDbFunctionParameterBuilder HasIndex(int index, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(_dbFunctionParameter.GetParameterIndexConfigurationSource())
                || _dbFunctionParameter.Index == index)
            {
                _dbFunctionParameter.SetParameterIndex(index, configurationSource);
            }

            return this;
        }
    }
}

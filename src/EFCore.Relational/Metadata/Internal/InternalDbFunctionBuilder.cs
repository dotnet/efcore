// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InternalDbFunctionBuilder
    {
        private readonly DbFunction _dbFunction;

        public InternalDbFunctionBuilder([NotNull] DbFunction dbFunction)
        {
            _dbFunction = dbFunction;
        }

        public virtual IMutableDbFunction Metadata => _dbFunction;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalDbFunctionBuilder HasSchema([CanBeNull] string schema, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(_dbFunction.GetSchemaConfigurationSource())
                || _dbFunction.Schema == schema)
            { 
                _dbFunction.SetSchema(schema, configurationSource);
            }

            return this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalDbFunctionBuilder HasName([NotNull] string name, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(_dbFunction.GetNameConfigurationSource())
                || _dbFunction.Name == name)
            {
                _dbFunction.SetName(name, configurationSource);
            }

            return this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalDbFunctionBuilder HasReturnType([NotNull] Type returnType, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(_dbFunction.GetReturnTypeConfigurationSource())
                || _dbFunction.ReturnType == returnType)
            {
                _dbFunction.SetReturnType(returnType, configurationSource);
            }

            return this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalDbFunctionParameterBuilder HasParameter([NotNull] string name, ConfigurationSource configurationSource)
        {
            return new InternalDbFunctionParameterBuilder(GetOrCreateParameter(name, configurationSource));
        }

        private DbFunctionParameter GetOrCreateParameter(string name, ConfigurationSource configurationSource)
        {
            return _dbFunction.FindParameter(name, configurationSource) ?? _dbFunction.AddParameter(name, configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalDbFunctionBuilder TranslateWith([NotNull] Func<IReadOnlyCollection<Expression>, IDbFunction, SqlFunctionExpression> translateCallback)
        {
            _dbFunction.TranslateCallback = translateCallback;

            return this;
        }
    }
}

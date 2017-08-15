// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InternalDbFunctionBuilder
    {
        private readonly DbFunction _function;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalDbFunctionBuilder([NotNull] DbFunction function)
        {
            _function = function;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IMutableDbFunction Metadata => _function;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalDbFunctionBuilder HasSchema([CanBeNull] string schema, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(_function.GetSchemaConfigurationSource())
                || _function.Schema == schema)
            {
                _function.SetSchema(schema, configurationSource);
            }

            return this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalDbFunctionBuilder HasName([NotNull] string name, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(_function.GetNameConfigurationSource())
                || _function.FunctionName == name)
            {
                _function.SetFunctionName(name, configurationSource);
            }

            return this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalDbFunctionBuilder HasTranslation([NotNull] Func<IReadOnlyCollection<Expression>, Expression> translation)
        {
            Check.NotNull(translation, nameof(translation));

            _function.Translation = translation;

            return this;
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring an <see cref="DbFunctionParameter" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class DbFunctionParameterBuilder 
    {
        private readonly InternalDbFunctionParameterBuilder _builder;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DbFunctionParameterBuilder([NotNull] DbFunctionParameter dbFunctionParameter)
        {
            Check.NotNull(dbFunctionParameter, nameof(dbFunctionParameter));

            _builder = new InternalDbFunctionParameterBuilder(dbFunctionParameter);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DbFunctionParameterBuilder([NotNull] InternalDbFunctionParameterBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            _builder = builder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IMutableDbFunctionParameter Metadata => _builder.Parameter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DbFunctionParameterBuilder HasIndex(int index)
        {
            _builder.HasIndex(index, ConfigurationSource.Explicit);
            
            return this;
        }
    }
}

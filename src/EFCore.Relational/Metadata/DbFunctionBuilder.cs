// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring a <see cref="Metadata" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    /// 
    public class DbFunctionBuilder
    {
        private readonly InternalDbFunctionBuilder _builder;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DbFunctionBuilder([NotNull] DbFunction dbFunction)
        {
            Check.NotNull(dbFunction, nameof(dbFunction));

            _builder = new InternalDbFunctionBuilder(dbFunction);
        }

        public virtual IMutableDbFunction Metadata => _builder.Metadata;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DbFunctionBuilder HasSchema([CanBeNull]string schema)
        {
            _builder.HasSchema(schema, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DbFunctionBuilder HasName([NotNull] string name)
        {
            Check.NotNull(name, nameof(name));

            _builder.HasName(name, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DbFunctionParameterBuilder HasParameter([NotNull] string name)
        {
            Check.NotNull(name, nameof(name));

            return new DbFunctionParameterBuilder(_builder.HasParameter(name, ConfigurationSource.Explicit));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DbFunctionBuilder HasReturnType([NotNull] Type returnType)
        {
            Check.NotNull(returnType, nameof(returnType));

            _builder.HasReturnType(returnType, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DbFunctionBuilder TranslateWith([NotNull] Func<IReadOnlyCollection<Expression>, IDbFunction, SqlFunctionExpression> translateCallback)
        {
            Check.NotNull(translateCallback, nameof(translateCallback));

            _builder.TranslateWith(translateCallback);

            return this;
        }
    }
}

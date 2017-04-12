// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring an <see cref="DbFunction" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    /// 
    public class DbFunctionBuilder : IInfrastructure<IMutableModel>, IInfrastructure<InternalDbFunctionBuilder>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DbFunctionBuilder([NotNull] InternalDbFunctionBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            Builder = builder;
        }

        private InternalDbFunctionBuilder Builder { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        IMutableModel IInfrastructure<IMutableModel>.Instance => Builder.ModelBuilder.Metadata;

        InternalDbFunctionBuilder IInfrastructure<InternalDbFunctionBuilder>.Instance => Builder;

        /// <summary>
        ///     The db function being configured.
        /// </summary>
        public virtual IDbFunction Metadata => Builder.Metadata;

        /// <summary>
        /// Sets the database schame the function exists in.
        /// </summary>
        /// <param name="schema">The name of the schema</param>
        /// <returns></returns>
        public virtual DbFunctionBuilder HasSchema([CanBeNull] string schema)
        {
            Builder.HasSchema(schema);

            return this;
        }

        /// <summary>
        /// Sets the name of the database function.
        /// </summary>
        /// <param name="name">The name of the function</param>
        /// <returns></returns>
        public virtual DbFunctionBuilder HasName([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            Builder.HasName(name);

            return this;
        }

        /// <summary>
        /// Sets a callback function which determines if a methodcall should be overridden by a EF Store call
        /// </summary>
        /// <param name="beforeCallback"></param>
        /// <returns></returns>
        public virtual DbFunctionBuilder BeforeInitialization([NotNull] Func<MethodCallExpression, IDbFunction, bool> beforeCallback)
        {
            Check.NotNull(beforeCallback, nameof(beforeCallback));

            Builder.BeforeInitialization(beforeCallback);

            return this;
        }

        /// <summary>
        /// Sets a callback which is executed after a methodcall is overridden by a EF Store call
        /// </summary>
        /// <param name="afterCallback"></param>
        /// <returns></returns>
        public virtual DbFunctionBuilder AfterInitialization([NotNull] Action<IDbFunction, DbFunctionExpression> afterCallback)
        {
            Check.NotNull(afterCallback, nameof(afterCallback));

            Builder.AfterInitialization(afterCallback);

            return this;
        }

        /// <summary>
        /// Sets a callback function which determines if a methodcall should be overridden by TSQL
        /// </summary>
        /// <param name="translateCallback"></param>
        /// <returns></returns>
        public virtual DbFunctionBuilder TranslateWith([NotNull] Func<IReadOnlyCollection<Expression>, IDbFunction, Expression> translateCallback)
        {
            Check.NotNull(translateCallback, nameof(translateCallback));

            Builder.TranslateWith(translateCallback);

            return this;
        }

        /// <summary>
        /// Retreives or Adds a db function parameter 
        /// </summary>
        /// <param name="name">The name of the function</param>
        /// <returns></returns>
        public virtual DbFunctionParameterBuilder Parameter([NotNull] string name)
            => new DbFunctionParameterBuilder(Builder.Parameter(name));

        /// <summary>
        /// Retreives or Adds a db function parameter 
        /// </summary>
        /// <param name="name">The name of the function</param>
        /// <returns></returns>
        public virtual DbFunctionBuilder RemoveParameter([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            Builder.RemoveParameter(name);

            return this;
        }
    }
}

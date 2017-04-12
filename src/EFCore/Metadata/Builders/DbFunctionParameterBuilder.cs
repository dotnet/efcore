// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
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
    public class DbFunctionParameterBuilder : IInfrastructure<IMutableModel>, IInfrastructure<InternalDbFunctionParameterBuilder>
    {
        private InternalDbFunctionParameterBuilder Builder { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DbFunctionParameterBuilder([NotNull] InternalDbFunctionParameterBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            Builder = builder;
        }

        /// <summary>
        ///     Sets the constant value for this parameter.
        /// </summary>
        /// <param name="value"> The constant values which is used for all invocations of the parent function. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionParameterBuilder HasValue([NotNull] object value)
        {
            Check.NotNull(value, nameof(value));

            Builder.HasValue(value);

            return this;
        }

        /// <summary>
        ///     Sets the index order for this parameter on the parent function
        /// </summary>
        /// <param name="index"> The index order </param>
        /// <param name="insert"> If true then all existing parameters on the parent funciton with an index >= the <paramref name="index"/> parameter will be increased by one. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionParameterBuilder HasParameterIndex(int index, bool insert = false)
        {
            Builder.HasParameterIndex(index, insert);

            return this;
        }

        /// <summary>
        ///     Sets whether to use the value of the instance for this parameter, for instance methods only.
        /// </summary>
        /// <param name="value"> Whether or not this parameter is an object </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionParameterBuilder IsObjectParameter(bool value)
        {
            Builder.IsObjectParameter(value);

            return this;
        }

        /// <summary>
        ///     Sets if this parameter is a database identifier.  Identifiers are inserted directly into into the underlying datastore command without modification.
        /// </summary>
        /// <param name="value"> Whether or not this parameter is an identifer </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionParameterBuilder IsIdentifier(bool value)
        {
            Builder.IsIdentifier(value);

            return this;
        }

        /// <summary>
        ///     Sets if this parameter represents a params parameter on the c# method.
        /// </summary>
        /// <param name="value"> Whether or not this parameter represents a params argument. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionParameterBuilder IsParams(bool value)
        {
            Builder.IsParams(value);

            return this;
        }

        /// <summary>
        ///     Sets the type of this parameter
        /// </summary>
        /// <param name="type"> The argument type. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionParameterBuilder HasType([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            Builder.HasType(type);

            return this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        IMutableModel IInfrastructure<IMutableModel>.Instance => Builder.ModelBuilder.Metadata;

        InternalDbFunctionParameterBuilder IInfrastructure<InternalDbFunctionParameterBuilder>.Instance => Builder;
    }
}

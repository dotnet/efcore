// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     Provides a simple API for configuring a <see cref="IMutableDbFunction" />.
    /// </summary>
    public abstract class DbFunctionBuilderBase : IInfrastructure<IConventionDbFunctionBuilder>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected DbFunctionBuilderBase([NotNull] IMutableDbFunction function)
        {
            Check.NotNull(function, nameof(function));

            Builder = ((DbFunction)function).Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual InternalDbFunctionBuilder Builder { [DebuggerStepThrough] get; }

        /// <inheritdoc />
        IConventionDbFunctionBuilder IInfrastructure<IConventionDbFunctionBuilder>.Instance
        {
            [DebuggerStepThrough] get => Builder;
        }

        /// <summary>
        ///     The function being configured.
        /// </summary>
        public virtual IMutableDbFunction Metadata
            => Builder.Metadata;

        /// <summary>
        ///     Sets the name of the database function.
        /// </summary>
        /// <param name="name"> The name of the function in the database. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionBuilderBase HasName([NotNull] string name)
        {
            Builder.HasName(name, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Sets the schema of the database function.
        /// </summary>
        /// <param name="schema"> The schema of the function in the database. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionBuilderBase HasSchema([CanBeNull] string schema)
        {
            Builder.HasSchema(schema, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Marks whether the database function is built-in.
        /// </summary>
        /// <param name="builtIn"> The value indicating whether the database function is built-in. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionBuilderBase IsBuiltIn(bool builtIn = true)
        {
            Builder.IsBuiltIn(builtIn, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Returns an object that can be used to configure a parameter with the given name.
        ///     If no parameter with the given name exists, then a new parameter will be added.
        /// </summary>
        /// <param name="name"> The parameter name. </param>
        /// <returns> The builder to use for further parameter configuration. </returns>
        public virtual DbFunctionParameterBuilder HasParameter([NotNull] string name)
            => new DbFunctionParameterBuilder(Builder.HasParameter(name, ConfigurationSource.Explicit).Metadata);

        #region Hidden System.Object members

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
            => base.ToString();

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectEqualsIsObjectEquals
        public override bool Equals(object obj)
            => base.Equals(obj);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode()
            => base.GetHashCode();

        #endregion
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     Provides a simple API for configuring a <see cref="IMutableDbFunction" />.
    /// </summary>
    public class DbFunctionBuilder : IInfrastructure<IConventionDbFunctionBuilder>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public DbFunctionBuilder([NotNull] IMutableDbFunction function)
        {
            Check.NotNull(function, nameof(function));

            Builder = ((DbFunction)function).Builder;
        }

        private InternalDbFunctionBuilder Builder { [DebuggerStepThrough] get; }

        /// <inheritdoc />
        IConventionDbFunctionBuilder IInfrastructure<IConventionDbFunctionBuilder>.Instance
        {
            [DebuggerStepThrough] get => Builder;
        }

        /// <summary>
        ///     The function being configured.
        /// </summary>
        public virtual IMutableDbFunction Metadata => Builder.Metadata;

        /// <summary>
        ///     Sets the name of the database function.
        /// </summary>
        /// <param name="name"> The name of the function in the database. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionBuilder HasName([NotNull] string name)
        {
            Builder.HasName(name, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Sets the schema of the database function.
        /// </summary>
        /// <param name="schema"> The schema of the function in the database. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionBuilder HasSchema([CanBeNull] string schema)
        {
            Builder.HasSchema(schema, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Sets the store type of the database function.
        /// </summary>
        /// <param name="storeType"> The store type of the function in the database. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionBuilder HasStoreType([CanBeNull] string storeType)
        {
            Builder.HasStoreType(storeType, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     <para>
        ///         Sets a callback that will be invoked to perform custom translation of this
        ///         function. The callback takes a collection of expressions corresponding to
        ///         the parameters passed to the function call. The callback should return an
        ///         expression representing the desired translation.
        ///     </para>
        ///     <para>
        ///         See https://go.microsoft.com/fwlink/?linkid=852477 for more information.
        ///     </para>
        /// </summary>
        /// <param name="translation"> The translation to use. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionBuilder HasTranslation([NotNull] Func<IReadOnlyCollection<SqlExpression>, SqlExpression> translation)
        {
            Builder.HasTranslation(translation, ConfigurationSource.Explicit);

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

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> true if the specified object is equal to the current object; otherwise, false. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectEqualsIsObjectEquals
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}

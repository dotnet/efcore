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
    ///     <para>
    ///         Provides a simple API for configuring a <see cref="DbFunctionParameter" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class DbFunctionParameterBuilder : IInfrastructure<IConventionDbFunctionParameterBuilder>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public DbFunctionParameterBuilder([NotNull] IMutableDbFunctionParameter parameter)
        {
            Check.NotNull(parameter, nameof(parameter));

            Builder = ((DbFunctionParameter)parameter).Builder;
        }

        private InternalDbFunctionParameterBuilder Builder { [DebuggerStepThrough] get; }

        /// <inheritdoc />
        IConventionDbFunctionParameterBuilder IInfrastructure<IConventionDbFunctionParameterBuilder>.Instance
        {
            [DebuggerStepThrough]
            get => Builder;
        }

        /// <summary>
        ///     The function parameter metadata that is being built.
        /// </summary>
        public virtual IMutableDbFunctionParameter Metadata
            => Builder.Metadata;

        /// <summary>
        ///     Sets the store type of the function parameter in the database.
        /// </summary>
        /// <param name="storeType"> The store type of the function parameter in the database. </param>
        /// <returns> The same builder instance so that further configuration calls can be chained. </returns>
        public virtual DbFunctionParameterBuilder HasStoreType([CanBeNull] string storeType)
        {
            Builder.HasStoreType(storeType, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Indicates whether parameter propagates nullability, meaning if it's value is null the database function itself returns null.
        /// </summary>
        /// <param name="propagatesNullability"> Value which indicates whether parameter propagates nullability. </param>
        /// <returns> The same builder instance so that further configuration calls can be chained. </returns>
        public virtual DbFunctionParameterBuilder PropagatesNullability(bool propagatesNullability = true)
        {
            Builder.PropagatesNullability(propagatesNullability, ConfigurationSource.Explicit);

            return this;
        }

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
            => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectEqualsIsObjectEquals
        public override bool Equals(object obj)
            => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode()
            => base.GetHashCode();

        #endregion
    }
}

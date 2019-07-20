// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
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
    public class DbFunctionParameterBuilder : IConventionDbFunctionParameterBuilder
    {
        private readonly DbFunctionParameter _parameter;

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

            _parameter = (DbFunctionParameter)parameter;
        }

        /// <summary>
        ///     The function parameter metadata that is being built.
        /// </summary>
        public virtual IMutableDbFunctionParameter Metadata => _parameter;

        /// <inheritdoc />
        IConventionDbFunctionParameter IConventionDbFunctionParameterBuilder.Metadata => _parameter;

        /// <summary>
        ///     Sets the store type of the function parameter in the database.
        /// </summary>
        /// <param name="storeType"> The store type of the function parameter in the database. </param>
        /// <returns> The same builder instance so that further configuration calls can be chained. </returns>
        public virtual DbFunctionParameterBuilder HasStoreType([CanBeNull] string storeType)
        {
            _parameter.StoreType = storeType;

            return this;
        }

        /// <inheritdoc />
        IConventionDbFunctionParameterBuilder IConventionDbFunctionParameterBuilder.HasStoreType(string storeType, bool fromDataAnnotation)
        {
            if (((IConventionDbFunctionParameterBuilder)this).CanSetStoreType(storeType, fromDataAnnotation))
            {
                ((IConventionDbFunctionParameter)_parameter).SetStoreType(storeType, fromDataAnnotation);
                return this;
            }

            return null;
        }

        /// <inheritdoc />
        bool IConventionDbFunctionParameterBuilder.CanSetStoreType(string storeType, bool fromDataAnnotation)
        {
            return Overrides(fromDataAnnotation, _parameter.GetStoreTypeConfigurationSource())
               || _parameter.StoreType == storeType;
        }

        /// <inheritdoc />
        IConventionDbFunctionParameterBuilder IConventionDbFunctionParameterBuilder.HasTypeMapping(
            RelationalTypeMapping typeMapping, bool fromDataAnnotation)
        {
            if (((IConventionDbFunctionParameterBuilder)this).CanSetTypeMapping(typeMapping, fromDataAnnotation))
            {
                ((IConventionDbFunctionParameter)_parameter).SetTypeMapping(typeMapping, fromDataAnnotation);
                return this;
            }

            return null;
        }

        /// <inheritdoc />
        bool IConventionDbFunctionParameterBuilder.CanSetTypeMapping(RelationalTypeMapping typeMapping, bool fromDataAnnotation)
        {
            return Overrides(fromDataAnnotation, _parameter.GetTypeMappingConfigurationSource())
               || _parameter.TypeMapping == typeMapping;
        }

        private bool Overrides(bool fromDataAnnotation, ConfigurationSource? configurationSource)
            => (fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                .Overrides(configurationSource);

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

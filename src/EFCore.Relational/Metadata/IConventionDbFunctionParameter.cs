// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public interface IConventionDbFunctionParameter : IDbFunctionParameter
    {
        new IConventionDbFunction Parent { get; }

        IConventionDbFunctionParameterBuilder Builder { get; }

        /// <summary>
        ///     Sets the store type of the parameter in the database.
        /// </summary>
        /// <param name="storeType"> The store type of the parameter in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        void SetStoreType([CanBeNull] string storeType, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="IDbFunctionParameter.StoreType" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IDbFunctionParameter.StoreType" />. </returns>
        ConfigurationSource? GetStoreTypeConfigurationSource();

        /// <summary>
        ///     Sets the type mapping of the parameter in the database.
        /// </summary>
        /// <param name="typeMapping"> The type mapping of the parameter in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        void SetTypeMapping([CanBeNull] RelationalTypeMapping typeMapping, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="IDbFunctionParameter.TypeMapping" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IDbFunctionParameter.TypeMapping" />. </returns>
        ConfigurationSource? GetTypeMappingConfigurationSource();

        /// <summary>
        ///     Sets if the parameter propagates null in the database.
        /// </summary>
        /// <param name="supportsNullPropagation"> The supports null propagation value. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        void SetSupportsNullPropagation(bool supportsNullPropagation, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="IDbFunctionParameter.SupportsNullPropagation" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IDbFunctionParameter.SupportsNullPropagation" />. </returns>
        ConfigurationSource? GetSupportsNullPropagationConfigurationSource();
    }
}

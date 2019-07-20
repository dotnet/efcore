// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a by-convention database function parameter in an <see cref="IConventionDbFunction" />.
    /// </summary>
    public interface IConventionDbFunctionParameter : IDbFunctionParameter
    {
        /// <summary>
        ///     The <see cref="IConventionDbFunction" /> to which this parameter belongs.
        /// </summary>
        new IConventionDbFunction Function { get; }

        /// <summary>
        ///     The <see cref="IConventionDbFunctionParameterBuilder"/> for building a by-convention function parameter.
        /// </summary>
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
    }
}

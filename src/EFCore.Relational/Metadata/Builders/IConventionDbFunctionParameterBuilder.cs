// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Provides a simple API for configuring a <see cref="IConventionDbFunctionParameter" />.
    /// </summary>
    public interface IConventionDbFunctionParameterBuilder
    {
        /// <summary>
        ///     The function parameter metadata that is being built.
        /// </summary>
        IConventionDbFunctionParameter Metadata { get; }

        /// <summary>
        ///     Sets the store type of the function parameter in the database.
        /// </summary>
        /// <param name="storeType"> The store type of the function parameter in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The same builder instance if the configuration was applied; null otherwise. </returns>
        IConventionDbFunctionParameterBuilder HasStoreType([CanBeNull] string storeType, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the store type can be set for this property
        ///     from the current configuration source.
        /// </summary>
        /// <param name="storeType"> The store type of the function parameter in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> True if the store type can be set for this property. </returns>
        bool CanSetStoreType([CanBeNull] string storeType, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the <see cref="RelationalTypeMapping" /> of the function parameter.
        /// </summary>
        /// <param name="typeMapping"> The type mapping to use for the function parameter. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The same builder instance if the configuration was applied; null otherwise. </returns>
        IConventionDbFunctionParameterBuilder HasTypeMapping(
            [CanBeNull] RelationalTypeMapping typeMapping, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether <see cref="RelationalTypeMapping" /> can be set for this property
        ///     from the current configuration source.
        /// </summary>
        /// <param name="typeMapping"> The type mapping to use for the function parameter. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> True if the type mapping can be set for this property. </returns>
        bool CanSetTypeMapping([CanBeNull] RelationalTypeMapping typeMapping, bool fromDataAnnotation = false);
    }
}

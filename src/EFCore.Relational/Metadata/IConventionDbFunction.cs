// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a relational database function in an <see cref="IConventionModel" /> in
    ///     the a form that can be mutated while the model is being built.
    /// </summary>
    public interface IConventionDbFunction : IDbFunction
    {
        /// <summary>
        ///     The <see cref="IConventionModel" /> in which this function is defined.
        /// </summary>
        new IConventionModel Model { get; }

        /// <summary>
        ///     The builder that can be used to configure this function.
        /// </summary>
        IConventionDbFunctionBuilder Builder { get; }

        /// <summary>
        ///     Returns the configuration source for this <see cref="IConventionDbFunction" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IConventionDbFunction" />. </returns>
        ConfigurationSource GetConfigurationSource();

        /// <summary>
        ///     Sets the name of the function in the database.
        /// </summary>
        /// <param name="name"> The name of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        void SetName([CanBeNull] string name, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="IDbFunction.Name" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IDbFunction.Name" />. </returns>
        ConfigurationSource? GetNameConfigurationSource();

        /// <summary>
        ///     Sets the schema of the function in the database.
        /// </summary>
        /// <param name="schema"> The schema of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        void SetSchema([CanBeNull] string schema, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="IDbFunction.Schema" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IDbFunction.Schema" />. </returns>
        ConfigurationSource? GetSchemaConfigurationSource();

        /// <summary>
        ///     Sets the store type of the function in the database.
        /// </summary>
        /// <param name="storeType"> The store type of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        void SetStoreType([CanBeNull] string storeType, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="IDbFunction.StoreType" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IDbFunction.StoreType" />. </returns>
        ConfigurationSource? GetStoreTypeConfigurationSource();

        /// <summary>
        ///     Sets the type mapping of the function in the database.
        /// </summary>
        /// <param name="typeMapping"> The type mapping of the function in the database. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        void SetTypeMapping([CanBeNull] RelationalTypeMapping typeMapping, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="IDbFunction.TypeMapping" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IDbFunction.TypeMapping" />. </returns>
        ConfigurationSource? GetTypeMappingConfigurationSource();

        /// <summary>
        ///     Sets the translation callback for performing custom translation of the method call into a SQL expression fragment.
        /// </summary>
        /// <param name="translation">
        ///     The translation callback for performing custom translation of the method call into a SQL expression fragment.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        void SetTranslation([CanBeNull] Func<IReadOnlyCollection<SqlExpression>, SqlExpression> translation, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="IDbFunction.Translation" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IDbFunction.Translation" />. </returns>
        ConfigurationSource? GetTranslationConfigurationSource();

        /// <summary>
        ///     The parameters for this function
        /// </summary>
        new IReadOnlyList<IConventionDbFunctionParameter> Parameters { get; }
    }
}

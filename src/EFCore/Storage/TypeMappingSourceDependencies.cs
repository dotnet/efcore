// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="TypeMappingSourceBase" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         Do not construct instances of this class directly from either provider or application code as the
    ///         constructor signature may change as new dependencies are added. Instead, use this type in
    ///         your constructor so that an instance will be created and injected automatically by the
    ///         dependency injection container. To create an instance with some dependent services replaced,
    ///         first resolve the object from the dependency injection container, then replace selected
    ///         services using the 'With...' methods. Do not call the constructor at any point in this process.
    ///     </para>
    /// </summary>
    public sealed class TypeMappingSourceDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="TypeMappingSourceBase" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        /// </summary>
        /// <param name="valueConverterSelector"> The registry of known <see cref="ValueConverter" />s. </param>
        /// <param name="plugins"> The plugins. </param>
        public TypeMappingSourceDependencies(
            [NotNull] IValueConverterSelector valueConverterSelector,
            [NotNull] IEnumerable<ITypeMappingSourcePlugin> plugins)
        {
            Check.NotNull(valueConverterSelector, nameof(valueConverterSelector));
            Check.NotNull(plugins, nameof(plugins));

            ValueConverterSelector = valueConverterSelector;
            Plugins = plugins;
        }

        /// <summary>
        ///     The registry of known <see cref="ValueConverter" />s.
        /// </summary>
        public IValueConverterSelector ValueConverterSelector { get; }

        /// <summary>
        ///     Gets the plugins.
        /// </summary>
        public IEnumerable<ITypeMappingSourcePlugin> Plugins { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="valueConverterSelector"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public TypeMappingSourceDependencies With([NotNull] IValueConverterSelector valueConverterSelector)
            => new TypeMappingSourceDependencies(valueConverterSelector, Plugins);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="plugins"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public TypeMappingSourceDependencies With([NotNull] IEnumerable<ITypeMappingSourcePlugin> plugins)
            => new TypeMappingSourceDependencies(ValueConverterSelector, plugins);
    }
}

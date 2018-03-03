// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     A simple default implementation of <see cref="ITypeMapper" />
    /// </summary>
    [Obsolete("Use TypeMappingSource")]
    public class CoreTypeMapper : ITypeMapper
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CoreTypeMapper" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public CoreTypeMapper([NotNull] CoreTypeMapperDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies used to create a <see cref="CoreTypeMapper" />
        /// </summary>
        protected virtual CoreTypeMapperDependencies Dependencies { get; }

        /// <summary>
        ///     Gets a value indicating whether the given .NET type is mapped.
        /// </summary>
        /// <param name="type"> The .NET type. </param>
        /// <returns> True if the type can be mapped; otherwise false. </returns>
        public virtual bool IsTypeMapped(Type type)
        {
            Check.NotNull(type, nameof(type));

            return type == typeof(string)
                   || type.GetTypeInfo().IsValueType
                   || type == typeof(byte[])
                   || Dependencies.ValueConverterSelector.Select(type).Any(c => IsTypeMapped(c.ProviderClrType));
        }
    }
}

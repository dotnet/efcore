// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     A simple default implementation of <see cref="ITypeMapper" />
    /// </summary>
    public class CoreTypeMapper : ITypeMapper
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CoreTypeMapper" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public CoreTypeMapper([NotNull] CoreTypeMapperDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));
        }

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
                   || type == typeof(byte[]);
        }

        /// <summary>
        ///     Describes metadata needed to decide on a type mapping for a property or type.
        /// </summary>
        protected class TypeMappingInfo
        {
            /// <summary>
            ///     Creates a new instance of <see cref="TypeMappingInfo" />.
            /// </summary>
            /// <param name="property"> The property for which mapping is needed. </param>
            /// <param name="modelClrType"> The CLR type in the model for which mapping is needed. </param>
            public TypeMappingInfo(
                [CanBeNull] IProperty property = null,
                [CanBeNull] Type modelClrType = null)
            {
                Property = property;
                ModelClrType = modelClrType ?? property?.ClrType;
            }

            /// <summary>
            ///     The property for which mapping is needed.
            /// </summary>
            public virtual IProperty Property { get; }

            /// <summary>
            ///     The CLR type in the model for which mapping is needed.
            /// </summary>
            public virtual Type ModelClrType { get; }
        }
    }
}

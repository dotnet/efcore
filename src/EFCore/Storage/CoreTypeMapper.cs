// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     A simple default implementation of <see cref="ITypeMapper" />
    /// </summary>
    public class CoreTypeMapper : ITypeMapper
    {
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
    }
}

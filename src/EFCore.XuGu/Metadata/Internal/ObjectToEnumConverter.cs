// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.XuGu.Metadata.Internal
{
    public static class ObjectToEnumConverter
    {
        /// <summary>
        /// Can be used to allow substitution of enum values with their underlying type in annotations, so that multi-provider models can
        /// be setup without provider specific dependencies.
        /// </summary>
        /// <remarks>
        /// See https://github.com/PomeloFoundation/Microsoft.EntityFrameworkCore.XuGu/issues/1205 for further information.
        /// </remarks>
        public static T? GetEnumValue<T>(object value)
            where T : struct
            => value != null &&
               Enum.IsDefined(typeof(T), value)
                ? (T?)(T)Enum.ToObject(typeof(T), value)
                : null;
    }
}

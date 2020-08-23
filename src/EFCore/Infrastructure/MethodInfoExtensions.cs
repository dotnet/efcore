// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Extension methods for <see cref="MethodInfo" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public static class MethodInfoExtensions
    {
        private static readonly string _efTypeName = typeof(EF).FullName;

        /// <summary>
        ///     Returns <see langword="true" /> if the given method is <see cref="EF.Property{TProperty}" />.
        /// </summary>
        /// <param name="methodInfo"> The method. </param>
        /// <returns> <see langword="true" /> if the method is <see cref="EF.Property{TProperty}" />; <see langword="false" /> otherwise. </returns>
        public static bool IsEFPropertyMethod([CanBeNull] this MethodInfo methodInfo)
            => Equals(methodInfo, EF.PropertyMethod)
                // fallback to string comparison because MethodInfo.Equals is not
                // always true in .NET Native even if methods are the same
                || methodInfo?.IsGenericMethod == true
                && methodInfo.Name == nameof(EF.Property)
                && methodInfo.DeclaringType?.FullName == _efTypeName;
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IMutableComplexTypeUsage" />.
    /// </summary>
    public static class ComplexTypeUsageExtensions
    {
        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        public static IComplexProperty FindProperty(
            [NotNull] this IComplexTypeUsage complexType, [NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(complexType, nameof(complexType));
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return complexType.FindProperty(propertyInfo.Name);
        }
    }
}

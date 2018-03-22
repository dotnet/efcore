// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class MetadataExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static TConcrete AsConcreteMetadataType<TInterface, TConcrete>([NotNull] TInterface @interface, [NotNull] string methodName)
            where TConcrete : class
        {
            if (!(@interface is TConcrete concrete))
            {
                throw new NotSupportedException(
                    CoreStrings.CustomMetadata(
                        methodName, typeof(TInterface).ShortDisplayName(), @interface.GetType().ShortDisplayName()));
            }

            return concrete;
        }
    }
}

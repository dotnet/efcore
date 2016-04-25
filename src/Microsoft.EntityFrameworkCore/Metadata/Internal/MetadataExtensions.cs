// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public static class MetadataExtensions
    {
        public static TConcrete AsConcreteMetadataType<TInterface, TConcrete>([NotNull] this TInterface @interface, [NotNull] string methodName)
            where TConcrete : class
        {
            var concrete = @interface as TConcrete;
            if (concrete == null)
            {
                throw new NotSupportedException(
                    CoreStrings.CustomMetadata(methodName, typeof(TInterface).Name, @interface.GetType().Name));
            }

            return concrete;
        }
    }
}

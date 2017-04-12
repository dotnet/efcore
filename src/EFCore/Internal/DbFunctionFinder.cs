// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class DbFunctionFinder
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IReadOnlyList<MethodInfo> FindFunctions([NotNull] DbContext context)
        {
            return context.GetType().GetRuntimeMethods()
                            .Where(m =>
                                    m.GetCustomAttributes(typeof(DbFunctionAttribute), false).Count() > 0)
                           .ToArray();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IReadOnlyList<MethodInfo> FindFunctions([NotNull] Type functionType)
        {
            return functionType.GetRuntimeMethods()
                            .Where(m =>
                                    m.GetCustomAttributes(typeof(DbFunctionAttribute), false).Count() > 0)
                           .ToArray();
        }
    }
}
